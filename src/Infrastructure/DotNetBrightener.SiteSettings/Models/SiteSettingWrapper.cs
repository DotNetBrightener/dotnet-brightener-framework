namespace DotNetBrightener.SiteSettings.Models;

public abstract class SiteSettingWrapper<TSetting> : SiteSettingBase
{   
    protected SiteSettingWrapper()
    {
        SettingType = typeof(TSetting).FullName;
    }

    public TSetting RetrieveSettings()
    {
        return RetrieveSettings<TSetting>();
    }
}