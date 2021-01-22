using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Runner.Processors;
using DotNetBrightener.Core.DataAccess.Abstractions;

namespace DotNetBrightener.Core.DataAccess.SchemaMigration.Extensions
{
    internal class MigrationProcessorAccessor : IProcessorAccessor
    {
        private readonly IEnumerable<IMigrationProcessor> _migrationProcessors;
        private readonly DatabaseConfiguration _databaseConfiguration;

        public MigrationProcessorAccessor(
            IEnumerable<IMigrationProcessor> migrationProcessors, 
            DatabaseConfiguration databaseConfiguration)
        {
            _migrationProcessors = migrationProcessors;
            _databaseConfiguration = databaseConfiguration;

            if (!_migrationProcessors.Any())
                throw new InvalidOperationException($"No migration processor available");

            var processor = _migrationProcessors.FirstOrDefault(_ =>
            _.DatabaseType.Equals(_databaseConfiguration.DatabaseProvider.ToString(), StringComparison.OrdinalIgnoreCase) ||
            _.DatabaseTypeAliases.Any(alias => alias.Equals(_databaseConfiguration.DatabaseProvider.ToString(), StringComparison.OrdinalIgnoreCase))
            );

            if (processor == null)
            {
                var generatorNames = string.Join(", ", _migrationProcessors.Select(p => p.DatabaseType).Union(_migrationProcessors.SelectMany(p => p.DatabaseTypeAliases)));

                throw new InvalidOperationException($"Migration processor with database type {_databaseConfiguration.DatabaseProvider} cound not be found. Available processors are: {generatorNames}");
            }

            Processor = processor;
        }

        public IMigrationProcessor Processor { get; private set; }
    }
}
