using DotNetBrightener.DataAccess.Models;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Models;

public abstract class SettingDescriptor: BaseEntity
{
    public abstract string SettingName { get; }

    public abstract string Description { get; }

    public abstract string DescriptionLocalizationKey { get; }

    public abstract string SettingNameLocalizationKey { get; }
}

public class SettingDescriptorModel: SettingDescriptor
{
    public override string SettingName { get; }

    public override string Description { get; }

    [JsonIgnore]
    public override string DescriptionLocalizationKey { get; }

    [JsonIgnore]
    public override string SettingNameLocalizationKey { get; }

    public string SettingType { get; set; }

    public SettingDescriptorModel(string settingName, string description)
    {
        SettingName = settingName;
        Description = description;
    }
}