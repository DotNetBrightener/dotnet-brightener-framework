using ActivityLog.Services;
using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;

namespace ActivityLog.DataStorage;

public class ActivityLogReadOnlyRepository(
    ActivityLogDbContext dbContext,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory)
    : ReadOnlyRepository(dbContext, serviceProvider, loggerFactory), IActivityLogReadOnlyRepository;