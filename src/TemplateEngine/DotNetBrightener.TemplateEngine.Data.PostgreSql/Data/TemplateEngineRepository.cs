using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;

internal class TemplateEngineRepository(
    TemplateEngineDbContext dbContext,
    IServiceProvider        serviceProvider,
    ILoggerFactory          loggerFactory)
    : Repository(dbContext, serviceProvider, loggerFactory);