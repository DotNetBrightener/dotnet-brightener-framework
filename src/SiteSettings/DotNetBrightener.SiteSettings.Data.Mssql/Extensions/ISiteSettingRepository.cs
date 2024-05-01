using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.SiteSettings.Data.Mssql.Data;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SiteSettings.Data.Mssql.Extensions;

internal interface ISiteSettingRepository : IRepository
{
}

internal class SiteSettingRepository(
    MssqlStorageSiteSettingDbContext dbContext,
    ICurrentLoggedInUserResolver     currentLoggedInUserResolver,
    IEventPublisher                  eventPublisher,
    ILoggerFactory                   loggerFactory)
    : Repository(dbContext, currentLoggedInUserResolver, eventPublisher, loggerFactory), ISiteSettingRepository;