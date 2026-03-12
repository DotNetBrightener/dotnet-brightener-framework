using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.SiteSettings.Entities;

namespace DotNetBrightener.SiteSettings;

public interface ISiteSettingDataService : IBaseDataService<SiteSettingRecord>;

public class SiteSettingDataService(IRepository repository)
    : BaseDataService<SiteSettingRecord>(repository), ISiteSettingDataService;