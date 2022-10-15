using DotNetBrightener.Core.DataAccess.Migration.Abstractions.Services;
using DotNetBrightener.Core.DataAccess.Migration.EntityFramework.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.DataAccess.Migration.EntityFramework.Services
{
    public class EFSchemaMigrationManager : ISchemaMigrationManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger          _logger;

        public EFSchemaMigrationManager(ILogger<EFSchemaMigrationManager> logger, 
                                        IServiceProvider serviceProvider)
        {
            _logger               = logger;
            _serviceProvider = serviceProvider;
        }

        public void ApplyUpMigrations(Assembly assembly)
        {
            var dbContextTypes = assembly.GetExportedTypes()
                                         .Where(_ => typeof(DbContext).IsAssignableFrom(_));

            var dbMigrationContexts = dbContextTypes.Select(_ => _serviceProvider.GetService(_))
                                                    .Cast<DbContext>();

            foreach (var dbMigrationContext in dbMigrationContexts)
            {
                dbMigrationContext.AutoMigrateDbSchema(_logger);
            }
        }

        public void ApplyDownMigrations(Assembly assembly)
        {
            throw new System.NotImplementedException("Backward migration with Entity Framework is not supported by our library");
        }
    }
}