namespace DotNetBrightener.SiteSettings.Data.Mssql.Extensions;

internal class SqlServerStorageSiteSettingDataService(ISiteSettingRepository repository)
    : SiteSettingDataService(repository);