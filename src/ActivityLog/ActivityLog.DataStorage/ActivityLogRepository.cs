using ActivityLog.Services;
using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;

namespace ActivityLog.DataStorage;

public class ActivityLogRepository(
    ActivityLogDbContext dbContext,
    IServiceProvider     serviceProvider,
    ILoggerFactory       loggerFactory) 
    : Repository(dbContext, serviceProvider, loggerFactory), IActivityLogRepository;