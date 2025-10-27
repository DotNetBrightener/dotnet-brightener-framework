using DotNetBrightener.DataAccess.DataMigration.Extensions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Transactions;

namespace DotNetBrightener.DataAccess.DataMigration;

internal class DataMigrationRunner(
    IServiceScopeFactory         serviceScopeFactory,
    ILogger<DataMigrationRunner> logger,
    IHostApplicationLifetime     lifetime)
    : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            MigrateSchemaIfNeeded(scope);
        }
        
        ExecuteMigration().Wait();
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

        var appliedMigrations = new List<DataMigrationHistory>();

        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            foreach (var migrationId in notAppliedMigrations)
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        logger.LogInformation("Applying data migration {migrationId}", migrationId);

                        await Migrate(scope, metadata, migrationId);

                        appliedMigrations.Add(new DataMigrationHistory
                        {
                            MigrationId    = migrationId,
                            AppliedDateUtc = DateTime.UtcNow
                        });

                        sw.Stop();

                        logger.LogInformation("Data migration {migrationId} applied in {elapsedTime}",
                                               migrationId,
                                               sw.Elapsed);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception,
                                         "Error while applying migration {migrationId}. Rolling back all changes.",
                                         migrationId);

                        throw;
                    }
                }
            }

            logger.LogInformation("Migrations applied. Saving history records...");

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                await using (var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>())
                {
                    await dbContext.BulkCopyAsync(appliedMigrations);
                }
            }

            logger.LogInformation("Successfully applied data migrations");
            transactionScope.Complete();
        }
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