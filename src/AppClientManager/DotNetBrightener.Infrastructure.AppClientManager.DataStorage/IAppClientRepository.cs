using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage;

public class AppClientRepository : Repository, IAppClientRepository
{
    public AppClientRepository(AppClientDbContext           dbContext,
                               ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                               IServiceProvider             serviceProvider,
                               ILoggerFactory               loggerFactory)
        : base(dbContext, serviceProvider, loggerFactory)
    {
    }
}