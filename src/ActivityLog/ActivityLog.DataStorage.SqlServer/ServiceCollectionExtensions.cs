using ActivityLog;
using ActivityLog.DataStorage;
using ActivityLog.DataStorage.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;

// ReSharper disable CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    extension(ActivityLogBuilder activityLogBuilder)
    {
        public ActivityLogBuilder UseSqlServer(string connectionString)
        {
            activityLogBuilder.UseSqlServer(((serviceProvider, options) => new SqlConnection(connectionString)));

            return activityLogBuilder;
        }

        public ActivityLogBuilder UseSqlServer(Func<IServiceProvider, DbContextOptionsBuilder, string>
                                                   connectionStringResolver)
        {
            activityLogBuilder.UseSqlServer((serviceProvider, options) =>
            {
                var connectionString =
                    connectionStringResolver.Invoke(serviceProvider, options);

                return new SqlConnection(connectionString);
            });

            return activityLogBuilder;
        }

        public ActivityLogBuilder UseSqlServer(Func<IServiceProvider, DbContextOptionsBuilder, DbConnection>
                                                   connectionStringResolver)
        {
            var services = activityLogBuilder.Services;

            services.UseDbContextWithMigration<ActivityLogDbContext, SqlServerMigrationDbContext>((serviceProvider,
                                                                                                   options) =>
            {
                var connection = connectionStringResolver.Invoke(serviceProvider, options);

                options.UseSqlServer(connection,
                                     x => x.MigrationsHistoryTable("__MigrationsHistory", nameof(ActivityLog)));
            });

            return activityLogBuilder;
        }
    }
}
