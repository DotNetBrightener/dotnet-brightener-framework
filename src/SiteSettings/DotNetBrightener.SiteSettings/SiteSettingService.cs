using DotNetBrightener.Caching;
using DotNetBrightener.SiteSettings.Abstractions;
using DotNetBrightener.SiteSettings.Entities;
using DotNetBrightener.SiteSettings.Extensions;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings;

public class SiteSettingService : ISiteSettingService
{
    private readonly ISiteSettingDataService      _dataService;
    private readonly IEnumerable<SiteSettingBase> _siteSettingInstances;
    private readonly IStringLocalizer             _stringLocalizer;
    private readonly ICacheManager                _cacheManager;
    private readonly IServiceProvider             _serviceProvider;

    public SiteSettingService(IEnumerable<SiteSettingBase>         siteSettingInstances,
                              ISiteSettingDataService              dataService,
                              IServiceProvider                     serviceProvider,
                              IStringLocalizer<SiteSettingService> stringLocalizer,
                              ICacheManager                        cacheManager)
    {
        _siteSettingInstances = siteSettingInstances;
        _dataService          = dataService;
        _serviceProvider      = serviceProvider;
        _stringLocalizer      = stringLocalizer;
        _cacheManager         = cacheManager;
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

        var defaultValue = _serviceProvider.TryGet(type);

        if (setting == null)
        {
            SaveSetting((SiteSettingBase)defaultValue, type);

            return defaultValue;
        }

        return setting.RetrieveSettingsWithDefaultMerge(type, defaultValue);
    }

    public void SaveSetting<T>(T value) where T : SiteSettingBase
    {
        SaveSetting(value, typeof(T));
    }

    public void SaveSetting(SiteSettingBase value, Type settingType)
    {
        ValidateSettingType(settingType);

        var settingKey = settingType.FullName;

        var setting = GetSiteSettingRecord(settingKey, false);

        value.UpdateSetting(setting);

        if (setting.Id == 0)
        {
            _dataService.Insert(setting);
        }
        else
        {
            _dataService.Update(setting);
        }

        _cacheManager.Remove(GetCacheKey(settingKey));
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
        var siteSettingRecord = fromCache
                                    ? _cacheManager.Get(GetCacheKey(settingKey),
                                                        () => InternalGetSiteSettingRecord(settingKey))
                                    : InternalGetSiteSettingRecord(settingKey);

        siteSettingRecord ??= new SiteSettingRecord
        {
            SettingType = settingKey
        };

        return siteSettingRecord;
    }

    private SiteSettingRecord InternalGetSiteSettingRecord(string settingKey)
    {
        return _dataService.Get(settingRecord => settingRecord.SettingType == settingKey);
    }


    private void ValidateSettingType(Type type)
    {
        if (!typeof(SiteSettingBase).IsAssignableFrom(type))
            throw new ArgumentException(string.Format(
                                                      $"{_stringLocalizer["SiteSettings.MsgError.SettingIsNotInherit"]}",
                                                      typeof(SiteSettingBase).FullName));
    }

    private static CacheKey GetCacheKey(string settingKey)
    {
        return new CacheKey($"SiteSettingService.GetSiteSettingRecord-{settingKey}", 10);
    }
}