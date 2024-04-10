using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.SiteSettings.Models;

public abstract class SiteSettingBase : SettingDescriptor
{
    [JsonIgnore]
    [NotMapped]
    public override string SettingName
    {

        get
        {
            if (T == null)
            {
                return SettingNameLocalizationKey;
            }

            var localizedString = T?[SettingNameLocalizationKey];

            return localizedString != SettingNameLocalizationKey
                       ? localizedString
                       : this.GetType().Name.CamelFriendly();
        }
    }

    [JsonIgnore]
    [NotMapped]
    public sealed override string Description
    {
        get
        {
            if (T == null)
            {
                return DescriptionLocalizationKey;
            }

            return T?[DescriptionLocalizationKey];
        }
    }

    public sealed override string SettingNameLocalizationKey => this.GetType().FullName + "-SettingName";

    [NotMapped]
    public override string DescriptionLocalizationKey => this.GetType().FullName + "-Description";

    [JsonIgnore]
    [NotMapped]
    public IStringLocalizer T { get; set; }

    [JsonIgnore]
    public string SettingType { get; set; }

    [JsonIgnore]
    public string SettingContent { get; set; }

    public T RetrieveSettings<T>()
    {
        if (RetrieveSettingsAsType(typeof(T)) is T tSetting)
        {
            return tSetting;
        }

        return default;
    }

    public object RetrieveSettingsAsType(Type type)
    {
        return JsonConvert.DeserializeObject(SettingContent, type);
    }

    public SettingDescriptorModel ToDescriptorModel()
    {
        return new SettingDescriptorModel(SettingName, Description)
        {
            SettingType = SettingType
        };
    }
}