using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.SiteSettings.Entities;

public class SiteSettingRecord : BaseEntityWithAuditInfo
{
    public string SettingType { get; set; }

    public string SettingContent { get; set; }
}