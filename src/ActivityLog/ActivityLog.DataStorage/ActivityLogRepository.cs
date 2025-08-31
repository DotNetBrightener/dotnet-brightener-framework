using ActivityLog.Services;
using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;

namespace ActivityLog.DataStorage;

public class ActivityLogRepository : Repository, IActivityLogRepository
{
    public ActivityLogRepository(ActivityLogDbContext           dbContext,
                                ICurrentLoggedInUserResolver   currentLoggedInUserResolver,
                                IServiceProvider               serviceProvider,
                                ILoggerFactory                 loggerFactory)
        : base(dbContext, serviceProvider, loggerFactory)
    {
    }
}
