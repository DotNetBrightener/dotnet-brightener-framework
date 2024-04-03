using DotNetBrightener.SiteSettings.Models;

namespace DotNetBrightener.SiteSettings.Abstractions;

public interface ISiteSettingService
{
    IEnumerable<SettingDescriptorModel> GetAllAvailableSettings();

    SiteSettingBase GetSettingInstance(string settingTypeName);

    T GetSetting<T>() where T : SiteSettingBase;

    dynamic GetSetting(Type type, bool isGetDefault = false);

    void SaveSetting<T>(T value) where T: SiteSettingBase;

    void SaveSetting<T>(T value, Type settingType) where T: SiteSettingBase;

    void SaveSetting(Type settingType, IDictionary<string, object> value);
}