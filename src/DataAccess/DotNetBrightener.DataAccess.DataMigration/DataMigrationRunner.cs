using DotNetBrightener.DataAccess.DataMigration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotNetBrightener.DataAccess.DataMigration;

internal class DataMigrationRunner(
    IServiceScopeFactory         serviceScopeFactory,
    ILogger<DataMigrationRunner> logger)
    : IHostedService, IDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            MigrateSchemaIfNeeded(scope);
        }

        try
        {
            await ExecuteMigration();
        }
        catch (Exception)
        {
            // Already logged inside ExecuteMigration: a migration failure must not fail app
            // startup, already-applied migrations stay committed and the rest retry next startup.
        }
    }

    private void MigrateSchemaIfNeeded(IServiceScope scope)
    {
        using (var dbContext = scope.ServiceProvider.GetRequiredService<DataMigrationDbContext>())
        {
            dbContext.AutoMigrateDbSchema(logger);
        }
    }

    private async Task ExecuteMigration()
    {
        List<string>               appliedMigrationIds;
        IOrderedEnumerable<string> allMigrationIds;
        DataMigrationMetadata      metadata;

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            metadata        = serviceProvider.GetRequiredService<DataMigrationMetadata>();
            allMigrationIds = metadata.Keys.Order();

            await using (var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>())
            {
                appliedMigrationIds = await dbContext.Set<DataMigrationHistory>()
                                                     .Select(h => h.MigrationId)
                                                     .ToListAsync();
            }
        }

        var notAppliedMigrations = allMigrationIds.Except(appliedMigrationIds)
                                                  .Order()
                                                  .ToArray();

        if (notAppliedMigrations.Length == 0)
        {
            logger.LogInformation("Data is up-to-date. No migration to be applied");

            return;
        }

        foreach (var migrationId in notAppliedMigrations)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    logger.LogInformation("Applying data migration {migrationId}", migrationId);

                    await Migrate(scope, metadata, migrationId);

                    var serviceProvider = scope.ServiceProvider;

                    await using (var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>())
                    {
                        await dbContext.AddAsync(new DataMigrationHistory
                        {
                            MigrationId    = migrationId,
                            AppliedDateUtc = DateTime.UtcNow
                        });

                        await dbContext.SaveChangesAsync();
                    }

                    sw.Stop();

                    logger.LogInformation("Data migration {migrationId} applied in {elapsedTime}",
                                           migrationId,
                                           sw.Elapsed);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception,
                                     "Error while applying migration {migrationId}. " +
                                     "Migrations already applied remain committed; " +
                                     "this migration and any remaining ones will be retried on next startup.",
                                     migrationId);

                    throw;
                }
            }
        }

        logger.LogInformation("Successfully applied data migrations");
    }

    private async Task Migrate(IServiceScope         scope,
                               DataMigrationMetadata metadata,
                               string                migrationId)
    {
        var serviceProvider = scope.ServiceProvider;

        var migration = metadata.GetMigration(serviceProvider, migrationId);

        if (migration != null)
        {
            await migration.MigrateData();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}