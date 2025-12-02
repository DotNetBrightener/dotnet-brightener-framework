using ActivityLog;
using ActivityLog.DataStorage;
using ActivityLog.DataStorage.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static ActivityLogBuilder UsePostgreSql(this ActivityLogBuilder activityLogBuilder,
                                                   string                  connectionString)
    {
        var services = activityLogBuilder.Services;

        services.UseDbContextWithMigration<ActivityLogDbContext, PostgreSqlMigrationDbContext>((serviceProvider,
                                                                                                options) =>
        {
            options.UseNpgsql(connectionString,
                              x => x.MigrationsHistoryTable("__MigrationsHistory", nameof(ActivityLog)));
        });

        return activityLogBuilder;
    }

    public static ActivityLogBuilder UsePostgreSql(this ActivityLogBuilder activityLogBuilder,
                                                   Func<IServiceProvider, DbContextOptionsBuilder, string>
                                                       connectionStringResolver)
    {
        var services = activityLogBuilder.Services;

        services.UseDbContextWithMigration<ActivityLogDbContext, PostgreSqlMigrationDbContext>((serviceProvider,
                                                                                                options) =>
        {
            var connectionString = connectionStringResolver.Invoke(serviceProvider, options);

            options.UseNpgsql(connectionString,
                              x => x.MigrationsHistoryTable("__MigrationsHistory", nameof(ActivityLog)));
        });

        return activityLogBuilder;
    }
}