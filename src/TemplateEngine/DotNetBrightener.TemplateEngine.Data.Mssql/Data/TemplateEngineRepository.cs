using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Data;

internal class TemplateEngineRepository : Repository
{
    public TemplateEngineRepository(TemplateEngineDbContext      dbContext,
                                    ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                                    IServiceProvider             serviceProvider,
                                    ILoggerFactory               loggerFactory)
        : base(dbContext, serviceProvider, loggerFactory)
    {
    }
}