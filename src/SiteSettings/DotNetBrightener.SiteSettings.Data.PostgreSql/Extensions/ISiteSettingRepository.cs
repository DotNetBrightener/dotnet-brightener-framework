using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.SiteSettings.Data.PostgreSql.Data;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SiteSettings.Data.PostgreSql.Extensions;

internal interface ISiteSettingRepository : IRepository;

internal class SiteSettingRepository(
    PostgreSqlStorageSiteSettingDbContext dbContext,
    IServiceProvider                 serviceProvider,
    ILoggerFactory                   loggerFactory)
    : Repository(dbContext, serviceProvider, loggerFactory), ISiteSettingRepository;