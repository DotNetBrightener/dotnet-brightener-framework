using DotNetBrightener.SiteSettings.Entities;
using DotNetBrightener.SiteSettings.Models;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Extensions;

public static class SiteSettingRecordExtensions
{
    public static object RetrieveSettingsWithDefaultMerge(this SiteSettingRecord settingRecord,
                                                          Type                   type,
                                                          object                 defaultValue)
    {
        var savedValue = JsonConvert.DeserializeObject<IDictionary<string, object>>(settingRecord.SettingContent);

        var serializedDefaultValue = JsonConvert.SerializeObject(defaultValue,
                                                                 settings: new JsonSerializerSettings
                                                                 {
                                                                     ContractResolver =
                                                                         new TypeOnlyContractResolver(type)
                                                                 });
        var defaultValueDictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(serializedDefaultValue);

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

    public static void UpdateSetting<TSetting>(this TSetting value, SiteSettingRecord record)
        where TSetting : SiteSettingBase
    {
        record.SettingContent = JsonConvert.SerializeObject(value,
                                                            settings: new JsonSerializerSettings
                                                            {
                                                                ContractResolver =
                                                                    new TypeOnlyContractResolver(value.GetType())
                                                            });
    }
}