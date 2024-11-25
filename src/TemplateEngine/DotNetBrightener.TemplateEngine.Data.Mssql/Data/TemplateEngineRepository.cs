using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Data;

internal class TemplateEngineRepository : Repository
{
    public TemplateEngineRepository(TemplateEngineDbContext dbContext,
                                    IServiceProvider        serviceProvider,
                                    ILoggerFactory          loggerFactory)
        : base(dbContext, serviceProvider, loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(GetType());
        
        dbContext.AutoMigrateDbSchema(logger);
    }
}