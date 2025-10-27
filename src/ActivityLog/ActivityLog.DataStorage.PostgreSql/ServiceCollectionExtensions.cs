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

        services.UseDbContextWithMigration<ActivityLogDbContext, PostgreSqlMigrationDbContext>(option =>
        {
            option.UseNpgsql(connectionString,
                             x => x.MigrationsHistoryTable("__MigrationsHistory", nameof(ActivityLog)));
        });

        return activityLogBuilder;
    }
}