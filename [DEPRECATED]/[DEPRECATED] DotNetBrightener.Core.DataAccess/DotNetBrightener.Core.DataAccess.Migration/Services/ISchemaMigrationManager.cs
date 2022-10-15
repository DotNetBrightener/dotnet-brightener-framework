using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetBrightener.Core.DataAccess.Migration.Abstractions.Services;

namespace DotNetBrightener.Core.DataAccess.Migration.Services
{
    public class SchemaMigrationManager : ISchemaMigrationManager
    {
        private readonly IFilteringMigrationSource _filteringMigrationSource;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IMigrationRunnerConventions _migrationRunnerConventions;
        private readonly Lazy<IVersionLoader> _versionLoader;

        public SchemaMigrationManager(IFilteringMigrationSource filteringMigrationSource,
                                      IMigrationRunner migrationRunner,
                                      IMigrationRunnerConventions migrationRunnerConventions,
                                      IServiceProvider serviceProvider)
        {
            _filteringMigrationSource = filteringMigrationSource;
            _migrationRunner = migrationRunner;
            _migrationRunnerConventions = migrationRunnerConventions;

            _versionLoader = new Lazy<IVersionLoader>(() => serviceProvider.GetService<IVersionLoader>());
        }

        public void ApplyUpMigrations(Assembly assembly)
        {
            var migrations = GetMigrations(assembly);

            foreach (var migrationInfo in migrations)
            {
                _migrationRunner.MigrateUp(migrationInfo.Version);
            }
        }

        public void ApplyDownMigrations(Assembly assembly)
        {
            var migrations = GetMigrations(assembly).Reverse();

            foreach (var migrationInfo in migrations)
            {
                _migrationRunner.Down(migrationInfo.Migration);
                _versionLoader.Value.DeleteVersion(migrationInfo.Version);
            }
        }

        private IEnumerable<IMigrationInfo> GetMigrations(Assembly assembly)
        {
            var migrations = _filteringMigrationSource.GetMigrations(t => assembly == null || t.Assembly == assembly) ?? Enumerable.Empty<IMigration>();

            return migrations.Select(m => _migrationRunnerConventions.GetMigrationInfoForMigration(m))
                             .OrderBy(migration => migration.Version);
        }
    }
}
