using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Data;

internal class TemplateEngineRepository : Repository
{
    public TemplateEngineRepository(TemplateEngineDbContext      dbContext,
                                    ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                                    IEventPublisher              eventPublisher,
                                    ILoggerFactory               loggerFactory)
        : base(dbContext, currentLoggedInUserResolver, eventPublisher, loggerFactory)
    {
    }
}