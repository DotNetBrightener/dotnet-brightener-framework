using DotNetBrightener.DataAccess.DataMigration.Extensions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Transactions;

namespace DotNetBrightener.DataAccess.DataMigration;

internal class DataMigrationRunner : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory         _serviceScopeFactory;
    private readonly ILogger<DataMigrationRunner> _logger;
    private readonly IHostApplicationLifetime     _lifetime;

    public DataMigrationRunner(IServiceScopeFactory         serviceScopeFactory,
                               ILogger<DataMigrationRunner> logger,
                               IHostApplicationLifetime     lifetime)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = logger;
        _lifetime            = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            MigrateSchemaIfNeeded(scope);
        }
        
        ExecuteMigration().Wait();
    }

    private void MigrateSchemaIfNeeded(IServiceScope scope)
    {
        using (var dbContext = scope.ServiceProvider.GetRequiredService<DataMigrationDbContext>())
        {
            var pendingMigrations = dbContext.Database
                                             .GetPendingMigrations()
                                             .ToArray();

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Migrating database for {dbContextName}", dbContext.GetType().Name);

                dbContext.Database.Migrate();

                _logger.LogInformation("Database migration completed for {dbContextName}", dbContext.GetType().Name);
            }
        }
    }

    private async Task ExecuteMigration()
    {
        List<string>               appliedMigrationIds;
        IOrderedEnumerable<string> allMigrationIds;
        DataMigrationMetadata      metadata;

        using (var scope = _serviceScopeFactory.CreateScope())
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
            _logger.LogInformation("Data is up-to-date. No migration to be applied");

            return;
        }

        var appliedMigrations = new List<DataMigrationHistory>();

        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            foreach (var migrationId in notAppliedMigrations)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        _logger.LogInformation("Applying data migration {migrationId}", migrationId);

                        await Migrate(scope, metadata, migrationId);

                        appliedMigrations.Add(new DataMigrationHistory
                        {
                            MigrationId    = migrationId,
                            AppliedDateUtc = DateTime.UtcNow
                        });

                        sw.Stop();

                        _logger.LogInformation("Data migration {migrationId} applied in {elapsedTime}",
                                               migrationId,
                                               sw.Elapsed);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception,
                                         "Error while applying migration {migrationId}. Rolling back all changes.",
                                         migrationId);

                        throw;
                    }
                }
            }

            _logger.LogInformation("Migrations applied. Saving history records...");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                await using (var dbContext = serviceProvider.GetRequiredService<DataMigrationDbContext>())
                {
                    await dbContext.BulkCopyAsync(appliedMigrations);
                }
            }

            _logger.LogInformation("Successfully applied data migrations");
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