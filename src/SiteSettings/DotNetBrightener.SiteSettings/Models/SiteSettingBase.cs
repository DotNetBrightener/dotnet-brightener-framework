using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Models;

public abstract class SiteSettingBase : SettingDescriptor
{
    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string SettingType { get; set; }

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
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