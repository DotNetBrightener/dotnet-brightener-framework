using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.SiteSettings.Models;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Entities;

public class SiteSettingRecord: SiteSettingBase
{
    public static SiteSettingRecord FromInstance<T>(T settingInstance) where T : SiteSettingBase
    {
        var siteSettingRecord = new SiteSettingRecord
        {
            SettingType = settingInstance.SettingType,
            T           = settingInstance.T,
        };

        siteSettingRecord.UpdateSetting(settingInstance);

        return siteSettingRecord;
    }

    public T GetSettingValueAs<T>() where T : class
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(SettingContent);
        }
        catch
        {
            return default(T);
        }
    }

    public void UpdateSettingValue(Type settingType, IDictionary<string, object> value)
    {
        if (GetSettingKey(settingType) != SettingType)
        {
            throw new Exception($@"Cannot save setting to a different type.
\n\nSetting type: {SettingType}\n\n
Value Type: {GetSettingKey(settingType)}");
        }

        try
        {
            var intermediateValue = JsonConvert.SerializeObject(value);

            var intermediateObject = JsonConvert.DeserializeObject(intermediateValue, settingType);

            SettingContent = JsonConvert.SerializeObject(intermediateObject);
        }
        catch
        {
            throw new Exception("Cannot serialize setting.");
        }
    }

    internal static string GetSettingKey(Type settingType)
    {
        return settingType.FullName;
    }

    [NotMapped]
    public override string SettingName
    {
        get
        {
            if (T == null)
                return SettingNameLocalizationKey;

            return T[SettingNameLocalizationKey];
        }
    }

    [NotMapped]
    public override string DescriptionLocalizationKey => null;
}