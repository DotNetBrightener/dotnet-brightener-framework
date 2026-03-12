using DotNetBrightener.SiteSettings.Models;

namespace DotNetBrightener.SiteSettings.Abstractions;

public interface ISiteSettingService
{
    IEnumerable<SettingDescriptorModel> GetAllAvailableSettings();

    SiteSettingBase GetSettingInstance(string settingTypeName);

    T       GetSetting<T>() where T : SiteSettingBase;
    
    Task<T> GetSettingAsync<T>() where T : SiteSettingBase;

    dynamic       GetSetting(Type      type, bool isGetDefault = false);
    Task<dynamic> GetSettingAsync(Type type, bool isGetDefault = false);

    void SaveSetting<T>(T      value) where T : SiteSettingBase;

    Task SaveSettingAsync<T>(T value) where T : SiteSettingBase;

    void SaveSetting(SiteSettingBase value, Type settingType);

    Task SaveSettingAsync(SiteSettingBase value, Type settingType);

    void SaveSetting(Type      settingType, Dictionary<string, object> value);

    Task SaveSettingAsync(Type settingType, Dictionary<string, object> value);
}