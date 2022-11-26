using DotNetBrightener.SiteSettings.Models;

namespace DotNetBrightener.CommonShared.Settings;

public class GenericSettings : SiteSettingWrapper<GenericSettings>
{
    public string SiteName { get; set; }

    public string SiteLogoUrl       { get; set; }

    public string SiteHomePageTitle { get; set; }

    public string SiteSlogan        { get; set; }

    public string PublicSiteUrl { get; set; }

    public string AdminSiteUrl { get; set; }

    public string ContactPhoneNumber { get; set; }

    public string ContactEmail { get; set; }

    public string ContactAddress { get; set; }

    public override string DescriptionLocalizationKey => "SiteSettings.Settings.GenericSettings";
}