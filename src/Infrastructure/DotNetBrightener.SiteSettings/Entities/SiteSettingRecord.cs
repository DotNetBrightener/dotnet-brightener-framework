using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.SiteSettings.Models;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Entities;

public class SiteSettingRecord : BaseEntityWithAuditInfo
{
    [MaxLength(2048)]
    public string SettingType { get; set; }

    public string SettingContent { get; set; }
}