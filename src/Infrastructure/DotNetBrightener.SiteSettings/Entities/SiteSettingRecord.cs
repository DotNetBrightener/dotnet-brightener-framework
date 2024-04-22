using DotNetBrightener.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.SiteSettings.Entities;

public class SiteSettingRecord : BaseEntityWithAuditInfo
{
    [MaxLength(2048)]
    public string SettingType { get; set; }

    public string SettingContent { get; set; }
}