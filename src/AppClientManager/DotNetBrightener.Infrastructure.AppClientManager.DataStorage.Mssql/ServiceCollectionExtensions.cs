using System.Data.Common;
using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage.Mssql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable CheckNamespace

namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    extension(AppClientManagerBuilder appClientManagerBuilder)
    {
        public AppClientManagerBuilder UseSqlServer(string connectionString)
        {
            appClientManagerBuilder.UseSqlServer((provider, builder) => new SqlConnection(connectionString));

            return appClientManagerBuilder;
        }

        public AppClientManagerBuilder UseSqlServer(Func<IServiceProvider, DbContextOptionsBuilder, DbConnection>
                                                        connectionStringResolver)
        {
            var services = appClientManagerBuilder.Services;

            services.UseDbContextWithMigration<AppClientDbContext, SqlServerMigrationDbContext>((serviceProvider,
                                                                                                 options) =>
            {
                var connection = connectionStringResolver.Invoke(serviceProvider, options);

                options.UseSqlServer(connection,
                                     x => x.MigrationsHistoryTable("__MigrationsHistory",
                                                                   AppClientDataDefaults
                                                                      .AppClientSchemaName));
            });

            return appClientManagerBuilder;
        }

        public AppClientManagerBuilder UseSqlServer(Func<IServiceProvider, DbContextOptionsBuilder, string>
                                                        connectionStringResolver)
        {
            appClientManagerBuilder.UseSqlServer((provider, builder) =>
            {
                var connectionString = connectionStringResolver.Invoke(provider, builder);

                return new SqlConnection(connectionString);
            });

            return appClientManagerBuilder;
        }
    }
}