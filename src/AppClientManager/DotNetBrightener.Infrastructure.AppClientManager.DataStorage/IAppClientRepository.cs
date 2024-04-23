using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage;

public class AppClientRepository : Repository, IAppClientRepository
{
    public AppClientRepository(AppClientDbContext           dbContext,
                               ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                               IEventPublisher              eventPublisher,
                               ILoggerFactory               loggerFactory)
        : base(dbContext, currentLoggedInUserResolver, eventPublisher, loggerFactory)
    {
    }
}