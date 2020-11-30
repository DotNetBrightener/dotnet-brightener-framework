using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.DataAccess.EF.Migrations.Schema
{
    /// <summary>
    ///     The service to execute the database schema migrations
    /// </summary>
    public interface ISchemaMigrationExecutor
    {
        /// <summary>
        ///     Migrates the databases which are used in the enabled modules
        /// </summary>
        Task MigrateDatabase();
    }

    public class SchemaMigrationExecutor : ISchemaMigrationExecutor
    {
        private readonly IEnumerable<DbContext> _dbMigrationContexts;
        private readonly ILogger                _logger;
        private readonly IConfiguration         _configuration;

        public SchemaMigrationExecutor(IEnumerable<DbContext>           dbMigrationContexts,
                                       IConfiguration                   configuration,
                                       ILogger<SchemaMigrationExecutor> logger)
        {
            _dbMigrationContexts = dbMigrationContexts;
            _configuration       = configuration;
            _logger              = logger;
        }

        public virtual Task MigrateDatabase()
        {
            var enableSchemaMigrationSetting = _configuration.GetValue<bool>("AutoMigrationSchemaEnabled");

            if (!enableSchemaMigrationSetting)
                return Task.CompletedTask;

            foreach (var dbMigrationContext in _dbMigrationContexts)
            {
                dbMigrationContext.AutoMigrateDbSchema(_logger);
            }

            return Task.CompletedTask;
        }
    }
}