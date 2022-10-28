using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

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

    public sealed override string SettingNameLocalizationKey => this.GetType().FullName + ".SettingName";

    [JsonIgnore]
    [NotMapped]
    public IStringLocalizer T { get; set; }

    [JsonIgnore]
    public string SettingType { get; set; }

    [JsonIgnore]
    public string SettingContent { get; set; }
        
    public DateTimeOffset? CreatedDate { get; set; }
        
    public string CreatedBy { get; set; }
        
    public DateTimeOffset? ModifiedDate { get; set; }

    public string          ModifiedBy   { get; set; }

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

    public object RetrieveSettingsWithDefaultMerge(Type type, object defaultValue)
    {
        var savedValue = JsonConvert.DeserializeObject<IDictionary<string, object>>(SettingContent);

        var serializedDefaultValue = JsonConvert.SerializeObject(defaultValue);

        var defaultValueDictionary =
            JsonConvert.DeserializeObject<IDictionary<string, object>>(serializedDefaultValue);

        foreach (var key in defaultValueDictionary.Keys)
        {
            if (!savedValue.ContainsKey(key))
            {
                savedValue.Add(key, defaultValueDictionary[key]);
            }
        }

        var serializedSavedValue = JsonConvert.SerializeObject(savedValue);

        return JsonConvert.DeserializeObject(serializedSavedValue, type);
    }

    public void UpdateSetting<T>(T settingValue)
    {
        SettingContent = JsonConvert.SerializeObject(settingValue);
    }

    public SettingDescriptorModel ToDescriptorModel()
    {
        return new SettingDescriptorModel(SettingName, Description)
        {
            SettingType = SettingType
        };
    }
}