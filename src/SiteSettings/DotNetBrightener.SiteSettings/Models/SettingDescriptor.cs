using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.SiteSettings.Models;

public abstract class SettingDescriptor
{
    [NotMapped]
    public abstract string SettingName { get; }

    [NotMapped]
    public abstract string Description { get; }
}

public class SettingDescriptorModel(string settingName, string description) : SettingDescriptor
{
    public override string SettingName { get; } = settingName;

    public override string Description { get; } = description;

    public string SettingType { get; set; }
}