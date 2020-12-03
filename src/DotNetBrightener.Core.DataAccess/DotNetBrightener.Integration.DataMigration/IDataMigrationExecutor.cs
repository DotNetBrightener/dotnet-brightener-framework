using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.Core.DataAccess;
using DotNetBrightener.Core.DataAccess.Repositories;
using DotNetBrightener.Core.DataAccess.Transaction;
using DotNetBrightener.Core.Modular;
using DotNetBrightener.Integration.DataMigration.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Integration.DataMigration
{
    /// <summary>
    ///     The service to perform the data migrations to the database
    /// </summary>
    public interface IDataMigrationExecutor
    {
        /// <summary>
        ///     Run the data update or modifications all enabled modules in the system
        /// </summary>
        Task UpdateData();
    }

    internal class DataMigrationExecutor : IDataMigrationExecutor
    {
        private readonly IBaseRepository                _repository;
        private readonly ITransactionManager            _transactionManager;
        private readonly IEnumerable<DataMigrationBase> _dataMigrationProviders;
        private readonly LoadedModuleEntries            _loadedModuleEntries;
        private readonly DatabaseConfiguration          _databaseConfiguration;
        private readonly IServiceProvider               _serviceProvider;
        private readonly ILogger                        _logger;

        public DataMigrationExecutor(IEnumerable<DataMigrationBase> dataMigrationProviders,
                                     ITransactionManager transactionManager,
                                     LoadedModuleEntries loadedModuleEntries,
                                     IBaseRepository repository,
                                     ILogger<DataMigrationExecutor> logger,
                                     IServiceProvider serviceProvider, 
                                     DatabaseConfiguration databaseConfiguration)
        {
            _dataMigrationProviders = dataMigrationProviders;
            _loadedModuleEntries = loadedModuleEntries;
            _logger = logger;
            _transactionManager = transactionManager;
            _repository = repository;
            _databaseConfiguration = databaseConfiguration;
            _serviceProvider = serviceProvider;
        }

        public Task UpdateData()
        {
            EnsureMigrationSchemaReady();

            if (!_dataMigrationProviders.Any())
                return Task.CompletedTask;

            foreach (var dataMigrationProvider in _dataMigrationProviders)
            {
                var associatedModule = _loadedModuleEntries.GetAssociatedModuleEntry(dataMigrationProvider);
                dataMigrationProvider.Prepare(associatedModule);
            }

            var orderedDataMigrationProviders =
                _dataMigrationProviders.OrderByModuleDependencies(_loadedModuleEntries)
                                       .GroupBy(_ => _.ModuleId);

            foreach (var orderedDataMigrationProvider in orderedDataMigrationProviders)
            {
                var moduleId           = orderedDataMigrationProvider.Key;
                var migrationProviders = orderedDataMigrationProvider.OrderBy(_ => _.MigrationId)
                                                                     .ToArray();

                try
                {
                    PerformMigration(moduleId, migrationProviders);
                }
                catch (Exception exception)
                {
                    // break the process if one migration failed
                    throw new DataMigrationException($"Error while applying data migration for the module {moduleId}",
                                                     exception);
                }
            }

            return Task.CompletedTask;
        }

        private void PerformMigration(string moduleId, DataMigrationBase [ ] migrationProviders)
        {
            var appliedMigrations = _repository.Fetch<DataMigrationHistoryRecord>(_ => _.ModuleId == moduleId)
                                               .ToList()
                                               .Select(_ => _.MigrationId);

            var pendingMigrations = migrationProviders.Where(_ => !appliedMigrations.Contains(_.MigrationId))
                                                      .OrderBy(_ => _.MigrationId);

            foreach (var pendingMigration in pendingMigrations)
            {
                using (var transaction = _transactionManager.BeginTransaction())
                {
                    try
                    {
                        pendingMigration.InternalUpgrade();
                        var dataMigrationHistoryRecord = new DataMigrationHistoryRecord
                        {
                            ModuleId          = pendingMigration.ModuleId,
                            MigrationId       = pendingMigration.MigrationId,
                            MigrationRecorded = DateTimeOffset.Now
                        };

                        _repository.Insert(dataMigrationHistoryRecord).Wait();
                    }
                    catch (Exception exception)
                    {
                        // roll back current migration, and break the migration process
                        transaction.Rollback();

                        throw new
                            DataMigrationException($"Error while applying data migration {pendingMigration.MigrationId} for module {pendingMigration.ModuleId}",
                                                   exception);
                    }
                }
            }
        }

        private void EnsureMigrationSchemaReady()
        {
            DbContext dataMigrationDbContext = _databaseConfiguration.DatabaseProvider switch
            {
                Core.DataAccess.DatabaseProvider.MsSql => _serviceProvider.GetService<DataMigrationMsSQLDbContext>(),
                Core.DataAccess.DatabaseProvider.PostgreSql => _serviceProvider.GetService<DataMigrationPostgreSQLDbContext>(),
                _ => null
            };

            if (dataMigrationDbContext == null)
                return;

            var databaseToMigrate = dataMigrationDbContext.Database;

            var pendingMigrations = databaseToMigrate.GetPendingMigrations()
                                                     .ToArray();

            if (pendingMigrations.Any())
            {
                var migrator = databaseToMigrate.GetService<IMigrator>();

                foreach (var pendingMigration in pendingMigrations)
                {
                    migrator.Migrate(pendingMigration);
                }
            }
        }
    }
}