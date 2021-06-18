using DotNetBrightener.Core.DataAccess.Migration.Abstractions.Services;
using DotNetBrightener.Core.DataAccess.Migration.EntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.DataAccess.Migration.EntityFramework.Extensions
{
    public static class SchemaMigrationEnabledServiceCollection
    {
        private static Action<DbContextOptionsBuilder> _configureDbContext = null;

        public static void EnableEFSchemaMigration(this IServiceCollection         serviceCollection,
                                                   Action<DbContextOptionsBuilder> configureDbContext,
                                                   params Assembly[]               assembliesContainMigrations)
        {
            serviceCollection.AddScoped<ISchemaMigrationManager, EFSchemaMigrationManager>();
            _configureDbContext = configureDbContext;

            var dbContextTypes = assembliesContainMigrations.SelectMany(_ => _.GetExportedTypes())
                                                            .Where(_ => typeof(DbContext).IsAssignableFrom(_));

            foreach (var dbContextType in dbContextTypes)
            {
                RegisterDbContext(serviceCollection, dbContextType);
            }
        }

        private static void RegisterDbContext(IServiceCollection serviceCollection,
                                              Type               dbContextType)
        {
            var dbContextOptionsBuilderType = typeof(DbContextOptionsBuilder<>);

            var genericType = dbContextOptionsBuilderType.MakeGenericType(dbContextType);

            if (Activator.CreateInstance(genericType) is DbContextOptionsBuilder dbContextOptions)
            {
                _configureDbContext?.Invoke(dbContextOptions);

                serviceCollection.AddSingleton(typeof(DbContextOptions<>).MakeGenericType(dbContextType),
                                               dbContextOptions.Options);
            }

            serviceCollection.AddScoped(dbContextType, dbContextType);
        }
    }
}