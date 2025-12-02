using ActivityLog;
using ActivityLog.DataStorage;
using ActivityLog.DataStorage.SqlServer;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static ActivityLogBuilder UseSqlServer(this ActivityLogBuilder activityLogBuilder,
                                                  string                  connectionString)
    {
        var services = activityLogBuilder.Services;

        services.UseDbContextWithMigration<ActivityLogDbContext, SqlServerMigrationDbContext>((serviceProvider,
                                                                                               options) =>
        {
            options.UseSqlServer(connectionString,
                                 x => x.MigrationsHistoryTable("__MigrationsHistory", nameof(ActivityLog)));
        });

        return activityLogBuilder;
    }

    public static ActivityLogBuilder UseSqlServer(this ActivityLogBuilder activityLogBuilder,
                                                  Func<IServiceProvider, DbContextOptionsBuilder, string>
                                                      connectionStringResolver)
    {
        var services = activityLogBuilder.Services;

        services.UseDbContextWithMigration<ActivityLogDbContext, SqlServerMigrationDbContext>((serviceProvider,
                                                                                               options) =>
        {
            var connectionString = connectionStringResolver.Invoke(serviceProvider, options);

            options.UseSqlServer(connectionString,
                                 x => x.MigrationsHistoryTable("__MigrationsHistory", nameof(ActivityLog)));
        });

        return activityLogBuilder;
    }
}
