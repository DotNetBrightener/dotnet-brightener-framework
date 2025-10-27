// ReSharper disable CheckNamespace

using ActivityLog;
using ActivityLog.DataStorage;
using ActivityLog.Services;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static ActivityLogBuilder WithStorage(this ActivityLogBuilder activityLogBuilder)
    {
        activityLogBuilder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        activityLogBuilder.Services.AddScoped<IActivityLogReadOnlyRepository, ActivityLogReadOnlyRepository>();
        activityLogBuilder.Services.AddScoped<IActivityLogDataService, ActivityLogDataService>();

        return activityLogBuilder;
    }

    public static ActivityLogBuilder UseInMemoryDatabase(this ActivityLogBuilder activityLogBuilder,
                                                         string                  databaseName)
    {
        var services = activityLogBuilder.Services;

        services.AddDbContext<ActivityLogDbContext>(options =>
                                                        options.UseInMemoryDatabase(databaseName));

        return activityLogBuilder;
    }
}
