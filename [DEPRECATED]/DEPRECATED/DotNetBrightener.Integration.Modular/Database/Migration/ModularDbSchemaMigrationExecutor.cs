using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetBrightener.Core.DataAccess.EF.Migrations.Schema;
using DotNetBrightener.Core.Modular;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Integration.Modular.Database.Migration
{
    public class ModularDbSchemaMigrationExecutor : ISchemaMigrationExecutor
    {
        private readonly IEnumerable<DbContext> _dbMigrationContexts;
        private readonly ILogger                _logger;
        private readonly LoadedModuleEntries    _loadedModuleEntries;
        private readonly IConfiguration         _configuration;

        public ModularDbSchemaMigrationExecutor(IEnumerable<DbContext>                    dbMigrationContexts,
                                                LoadedModuleEntries                       loadedModuleEntries,
                                                IConfiguration                            configuration,
                                                ILogger<ModularDbSchemaMigrationExecutor> logger)
        {
            _dbMigrationContexts = dbMigrationContexts;
            _configuration       = configuration;
            _loadedModuleEntries = loadedModuleEntries;
            _logger              = logger;
        }

        public virtual Task MigrateDatabase()
        {
            var enableSchemaMigrationSetting = _configuration.GetValue<bool>("AutoMigrationSchemaEnabled");

            if (!enableSchemaMigrationSetting)
                return Task.CompletedTask;

            // order the migration contexts by module tree
            var orderedMigrationContexts = _dbMigrationContexts.OrderByModuleDependencies(_loadedModuleEntries);

            foreach (var dbMigrationContext in orderedMigrationContexts)
            {
                dbMigrationContext.AutoMigrateDbSchema(_logger);
            }

            return Task.CompletedTask;
        }
    }
}