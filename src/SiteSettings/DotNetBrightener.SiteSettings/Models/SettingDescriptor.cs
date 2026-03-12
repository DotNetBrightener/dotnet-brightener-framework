using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Models;

public abstract class SettingDescriptor
{
    [NotMapped]
    public abstract string SettingName { get; }

    [NotMapped]
    public abstract string SettingDescription { get; }

    [Obsolete("Will be removed in future versions, use SettingDescription instead.")]
    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string Description => SettingDescription;
}

public class SettingDescriptorModel(string settingName, string description) : SettingDescriptor
{
    public override string SettingName { get; } = settingName;

    public override string SettingDescription { get; } = description;

    public string SettingType { get; set; }
}