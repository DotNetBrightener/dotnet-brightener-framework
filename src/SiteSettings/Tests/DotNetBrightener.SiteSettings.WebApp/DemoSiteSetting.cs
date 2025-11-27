using DotNetBrightener.SiteSettings.Models;

namespace DotNetBrightener.SiteSettings.WebApp;

public class DemoSiteSetting : SiteSettingWrapper<DemoSiteSetting>
{
    public string StringSetting { get; set; }

    public int IntSetting { get; set; }

    public string Defaultvalue { get; set; } = "Default Value";

    public override string SettingName => "Demo Site Settings";

    public override string SettingDescription => "Demo Site Settings";
}