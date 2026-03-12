using DotNetBrightener.Caching;
using DotNetBrightener.SiteSettings.Abstractions;
using DotNetBrightener.SiteSettings.Entities;
using DotNetBrightener.SiteSettings.Extensions;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings;

public class SiteSettingService(
    IEnumerable<SiteSettingBase>         siteSettingInstances,
    ISiteSettingDataService              dataService,
    IServiceProvider                     serviceProvider,
    IStringLocalizer<SiteSettingService> stringLocalizer,
    ICacheManager                        cacheManager)
    : ISiteSettingService
{
    private readonly IStringLocalizer _stringLocalizer = stringLocalizer;

    public IEnumerable<SettingDescriptorModel> GetAllAvailableSettings()
    {
        return siteSettingInstances.Select(x => x.ToDescriptorModel());
    }

    public SiteSettingBase GetSettingInstance(string settingTypeName)
    {
        return siteSettingInstances.FirstOrDefault(x => x.SettingType == settingTypeName);
    }

    public T GetSetting<T>() where T : SiteSettingBase
    {
        return GetSetting(typeof(T));
    }

    public async Task<T> GetSettingAsync<T>() where T : SiteSettingBase
    {
        var settingResult = await GetSettingAsync(typeof(T));

        if (settingResult is T tSetting)
            return tSetting;

        return null;
    }

    public dynamic GetSetting(Type type, bool isGetDefault = false)
        => GetSettingAsync(type, isGetDefault).Result;

    public async Task<dynamic> GetSettingAsync(Type type, bool isGetDefault = false)
    {
        ValidateSettingType(type);

        var defaultValue = serviceProvider.TryGet(type);

        if (isGetDefault)
            return defaultValue;

        var settingKey = type.FullName;

        var setting = await GetSiteSettingRecordAsync(settingKey);

        if (setting == null ||
            setting.Id == 0)
        {
            await SaveSettingAsync((SiteSettingBase)defaultValue, type);

            return defaultValue;
        }

        return setting.RetrieveSettingsWithDefaultMerge(type, defaultValue);
    }

    public void SaveSetting<T>(T value) where T : SiteSettingBase
        => SaveSettingAsync(value).Wait();

    public Task SaveSettingAsync<T>(T value) where T : SiteSettingBase
        => SaveSettingAsync(value, typeof(T));

    public void SaveSetting(SiteSettingBase value, Type settingType)
        => SaveSettingAsync(value, settingType).Wait();

    public async Task SaveSettingAsync(SiteSettingBase value, Type settingType)
    {
        ValidateSettingType(settingType);

        var settingKey = settingType.FullName;

        var setting = await GetSiteSettingRecordAsync(settingKey, false);

        value.UpdateSetting(setting);

        if (setting.Id == 0)
        {
            await dataService.InsertAsync(setting);
        }
        else
        {
            await dataService.UpdateAsync(setting);
        }

        cacheManager.Remove(GetCacheKey(settingKey));
    }

    public void SaveSetting(Type settingType, Dictionary<string, object> value)
        => SaveSettingAsync(settingType, value).Wait();

    public async Task SaveSettingAsync(Type settingType, Dictionary<string, object> value)
    {
        if (!typeof(SiteSettingBase).IsAssignableFrom(settingType))
            throw new ArgumentException(string.Format(
                                                      $"{_stringLocalizer["SiteSettings.MsgError.SettingIsNotInherit"]}",
                                                      typeof(SiteSettingBase).FullName));

        var serializeObject = JsonConvert.SerializeObject(value);
        var settingValue    = JsonConvert.DeserializeObject(serializeObject, settingType);

        if (settingValue is SiteSettingBase setting)
        {
            await SaveSettingAsync(setting, settingType);
        }
        else
        {
            throw new
                InvalidOperationException($"Could not save the setting of type {settingType.FullName} with given values");
        }
    }

    private async Task<SiteSettingRecord> GetSiteSettingRecordAsync(string settingKey, bool fromCache = true)
    {
        var siteSettingRecord = fromCache
                                    ? await cacheManager.GetAsync(GetCacheKey(settingKey),
                                                                  () => InternalGetSiteSettingRecordAsync(settingKey))
                                    : await InternalGetSiteSettingRecordAsync(settingKey);

        siteSettingRecord ??= new SiteSettingRecord
        {
            SettingType = settingKey
        };

        return siteSettingRecord;
    }

    private async Task<SiteSettingRecord> InternalGetSiteSettingRecordAsync(string settingKey)
    {
        return await dataService.GetAsync(settingRecord => settingRecord.SettingType == settingKey);
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