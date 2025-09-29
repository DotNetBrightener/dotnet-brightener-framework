using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.SiteSettings.Entities;

namespace DotNetBrightener.SiteSettings;

public interface ISiteSettingDataService : IBaseDataService<SiteSettingRecord>;

public class SiteSettingDataService : BaseDataService<SiteSettingRecord>, ISiteSettingDataService
{
    public SiteSettingDataService(IRepository repository)
        : base(repository)
    {
    }
}