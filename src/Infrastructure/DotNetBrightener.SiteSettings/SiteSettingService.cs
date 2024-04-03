using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.SiteSettings.Abstractions;
using DotNetBrightener.SiteSettings.Entities;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings;

public class SiteSettingService : ISiteSettingService
{
    private readonly IRepository                  _repository;
    private readonly IEnumerable<SiteSettingBase> _siteSettingInstances;
    private readonly IStringLocalizer             _stringLocalizer;
    private readonly IServiceProvider             _serviceProvider;

    public SiteSettingService(IEnumerable<SiteSettingBase>         siteSettingInstances,
                              IRepository                          repository,
                              IServiceProvider                     serviceProvider,
                              IStringLocalizer<SiteSettingService> stringLocalizer)
    {
        _siteSettingInstances = siteSettingInstances;
        _repository           = repository;
        _serviceProvider      = serviceProvider;
        _stringLocalizer      = stringLocalizer;
    }

    public IEnumerable<SettingDescriptorModel> GetAllAvailableSettings()
    {
        return _siteSettingInstances.Select(_ => _.ToDescriptorModel());
    }

    public SiteSettingBase GetSettingInstance(string settingTypeName)
    {
        return _siteSettingInstances.FirstOrDefault(_ => _.SettingType == settingTypeName);
    }

    public T GetSetting<T>() where T : SiteSettingBase
    {
        return GetSetting(typeof(T));
    }

    public dynamic GetSetting(Type type, bool isGetDefault = false)
    {
        ValidateSettingType(type);

        var settingKey = type.FullName;

        var setting = GetSiteSettingRecord(settingKey);

        var defaultValue = _serviceProvider.GetService(type);

        if (setting == null)
            return defaultValue;

        return setting.RetrieveSettingsWithDefaultMerge(type, defaultValue);
    }

    public void SaveSetting<T>(T value) where T : SiteSettingBase
    {
        SaveSetting(value, typeof(T));
    }

    public void SaveSetting<T>(T value, Type settingType) where T : SiteSettingBase
    {
        ValidateSettingType<T>();

        var settingKey = settingType.FullName;

        var setting = GetSiteSettingRecord(settingKey, false);

        if (setting == null)
        {
            setting = new SiteSettingRecord
            {
                SettingType = settingKey
            };
            setting.UpdateSetting(value);
            _repository.Insert(setting);
        }
        else
        {
            setting.UpdateSetting(value);
            _repository.Update(setting);
        }

        _repository.CommitChanges();
    }

    public void SaveSetting(Type settingType, IDictionary<string, object> value)
    {
        if (!typeof(SiteSettingBase).IsAssignableFrom(settingType))
            throw new ArgumentException(string.Format(
                                                      $"{_stringLocalizer["SiteSettings.MsgError.SettingIsNotInherit"]}",
                                                      typeof(SiteSettingBase).FullName));

        var settingValue = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), settingType);

        if (settingValue is SiteSettingBase setting)
        {
            SaveSetting(setting, settingType);
        }
        else
        {
            throw new
                InvalidOperationException($"Could not save the setting of type {settingType.FullName} with given values");
        }
    }

    private SiteSettingRecord GetSiteSettingRecord(string settingKey, bool fromCache = true)
    {
        var siteSettingRecord = InternalGetSiteSettingRecord(settingKey);

        return siteSettingRecord;
    }

    private SiteSettingRecord InternalGetSiteSettingRecord(string settingKey)
    {
        return _repository
           .Get<SiteSettingRecord>(_ => _.SettingType == settingKey);

    }

    private void ValidateSettingType<T>()
    {
        ValidateSettingType(typeof(T));
    }

    private void ValidateSettingType(Type type)
    {
        if (!typeof(SiteSettingBase).IsAssignableFrom(type))
            throw new ArgumentException(string.Format(
                                                      $"{_stringLocalizer["SiteSettings.MsgError.SettingIsNotInherit"]}",
                                                      typeof(SiteSettingBase).FullName));
    }
}