using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Initialization;
using System;
using DotNetBrightener.Core.DataAccess.Abstractions;

namespace DotNetBrightener.Core.DataAccess.SchemaMigration.Extensions
{
    internal class DataConnectionStringAccessor : IConnectionStringAccessor
    {
        public DataConnectionStringAccessor(DatabaseConfiguration databaseConfiguration)
        {
            ConnectionString = databaseConfiguration.ConnectionString;
        }

        public string ConnectionString { get; private set; }
    }

    public static class SchemaMigrationEnabledServiceCollection
    {
        public static void EnableSchemaMigration(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddFluentMigratorCore()
                .AddScoped<IProcessorAccessor, MigrationProcessorAccessor>()
                .AddScoped<IConnectionStringAccessor>(serviceProvider => RetrieveConnectionStringAccessor(serviceProvider))
                .ConfigureRunner(runnerBuilder =>
                {
                    runnerBuilder.WithVersionTable(new SchemaMigrationHistory())
                                 .AddSqlServer()
                                 .AddPostgres()
                                 .AddMySql5();
                });
        }

        private static IConnectionStringAccessor RetrieveConnectionStringAccessor(IServiceProvider serviceProvider)
        {
            return new DataConnectionStringAccessor(serviceProvider.GetService<DatabaseConfiguration>());
        }
    }
}
