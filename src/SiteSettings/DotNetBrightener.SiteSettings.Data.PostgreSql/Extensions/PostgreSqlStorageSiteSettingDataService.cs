namespace DotNetBrightener.SiteSettings.Data.PostgreSql.Extensions;

internal class PostgreSqlStorageSiteSettingDataService(ISiteSettingRepository repository)
    : SiteSettingDataService(repository);