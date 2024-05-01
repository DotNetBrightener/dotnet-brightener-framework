using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.SiteSettings.Models;

public abstract class SettingDescriptor
{
    [NotMapped]
    public abstract string SettingName { get; }

    [NotMapped]
    public abstract string Description { get; }
}

public class SettingDescriptorModel: SettingDescriptor
{
    public override string SettingName { get; }

    public override string Description { get; }

    public string SettingType { get; set; }

    public SettingDescriptorModel(string settingName, string description)
    {
        SettingName = settingName;
        Description = description;
    }
}