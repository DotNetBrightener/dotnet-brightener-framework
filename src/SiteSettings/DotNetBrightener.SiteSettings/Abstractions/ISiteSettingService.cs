using DotNetBrightener.SiteSettings.Models;

namespace DotNetBrightener.SiteSettings.Abstractions;

public interface ISiteSettingService
{
    IEnumerable<SettingDescriptorModel> GetAllAvailableSettings();

    SiteSettingBase GetSettingInstance(string settingTypeName);

    T GetSetting<T>() where T : SiteSettingBase;

    dynamic GetSetting(Type type, bool isGetDefault = false);

    void SaveSetting<T>(T value) where T: SiteSettingBase;

    void SaveSetting(SiteSettingBase value, Type settingType);

    void SaveSetting(Type settingType, Dictionary<string, object> value);
}