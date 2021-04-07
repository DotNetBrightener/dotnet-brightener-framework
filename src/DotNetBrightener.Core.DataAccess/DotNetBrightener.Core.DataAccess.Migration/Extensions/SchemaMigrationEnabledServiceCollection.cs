using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.Migration.Services;
using DotNetBrightener.Core.DataAccess.SchemaMigration.Extensions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.DataAccess.Migration.Extensions
{

    public static class SchemaMigrationEnabledServiceCollection
    {
        public static void EnableSchemaMigration(this IServiceCollection serviceCollection,
            params Assembly[] assembliesContainMigrations)
        {
            serviceCollection.AddScoped<ISchemaMigrationManager, SchemaMigrationManager>();

            serviceCollection.AddFluentMigratorCore()
                .AddScoped<IProcessorAccessor, MigrationProcessorAccessor>()
                .AddScoped(serviceProvider => RetrieveConnectionStringAccessor(serviceProvider))
                .ConfigureRunner(runnerBuilder =>
                {
                    runnerBuilder.WithVersionTable(new SchemaMigrationHistory())
                                 .AddSqlServer()
                                 .AddPostgres()
                                 .AddMySql5();

                    if (assembliesContainMigrations?.Any() == true)
                    {
                        runnerBuilder.ScanIn(assembliesContainMigrations).For.Migrations();
                    }
                });
        }

        private static IConnectionStringAccessor RetrieveConnectionStringAccessor(IServiceProvider serviceProvider)
        {
            return new DataConnectionStringAccessor(serviceProvider.GetService<DatabaseConfiguration>());
        }
    }
}
