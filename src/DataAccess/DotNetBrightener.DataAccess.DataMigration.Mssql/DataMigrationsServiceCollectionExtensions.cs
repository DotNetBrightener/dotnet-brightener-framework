using DotNetBrightener.DataAccess.DataMigration;
using DotNetBrightener.DataAccess.DataMigration.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DataMigrationsServiceCollectionExtensions
{
    extension(DataMigrationConfiguration configuration)
    {
        public DataMigrationConfiguration UseSqlServer(string                     connectionString)
        {
            configuration.ConnectionString = connectionString;
            configuration.ServiceCollection.AddDbContext<DataMigrationDbContext>(options =>
            {
                options.UseSqlServer(connectionString,
                                     x =>
                                     {
                                         x.MigrationsHistoryTable(configuration.MigrationHistoryTableName,
                                                                  configuration.MigrationHistoryTableSchema);

                                         x.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                                     });
            });

            return configuration;
        }

        public DataMigrationConfiguration UseSqlServer(Func<IServiceProvider, DbContextOptionsBuilder, DbConnection>
                                                           connectionStringResolver)
        {
            configuration.ServiceCollection.AddDbContext<DataMigrationDbContext>((serviceProvider, options) =>
            {
                var connection = connectionStringResolver.Invoke(serviceProvider, options);

                options.UseSqlServer(connection,
                                     x =>
                                     {
                                         x.MigrationsHistoryTable(configuration.MigrationHistoryTableName,
                                                                  configuration.MigrationHistoryTableSchema);

                                         x.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                                     });
            });

            return configuration;
        }
    }
}