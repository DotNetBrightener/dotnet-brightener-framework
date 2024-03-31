using DotNetBrightener.Caching;
using LanguageExts.Results;
using LocaleManagement.Data;
using LocaleManagement.Entities;
using LocaleManagement.ErrorMessages;
using LocaleManagement.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LocaleManagement.Services;

public class LocaleManagementService : ILocaleManagementService
{
    private readonly ICacheManager                   _cacheManager;
    private readonly IAppLocaleDictionaryDataService _appLocaleDictionaryDataService;
    private readonly IDictionaryEntryDataService     _dictionaryEntryDataService;
    private readonly ILogger                         _logger;

    public LocaleManagementService(IAppLocaleDictionaryDataService  appLocaleDictionaryDataService,
                                   IDictionaryEntryDataService      dictionaryEntryDataService,
                                   ILogger<LocaleManagementService> logger,
                                   ICacheManager                    cacheManager)
    {
        _appLocaleDictionaryDataService = appLocaleDictionaryDataService;
        _dictionaryEntryDataService     = dictionaryEntryDataService;
        _cacheManager                   = cacheManager;
        _logger                         = logger;
    }

    public Task<List<SupportedLocale>> GetSystemSupportedLocales(params string[] countryCodes)
    {
        var result = _cacheManager.Get(GetSystemSupportedLocalesCacheKey(countryCodes),
                                       () => InternalGetSystemSupportedLocales(countryCodes)
                                      );

        return Task.FromResult(result);
    }

    public async Task<List<SupportedLocale>> GetSupportedLocalesByAppId(string appId)
    {
        var cacheKey = GetAppSupportedLocalesCacheKey(appId);

        var result = _cacheManager.Get(cacheKey, () => InternalGetSupportedLocalesByAppId(appId));

        return result;
    }

    public async Task<Result<Dictionary<string, string>>>
        GetDictionaryEntriesByLocale(string appId, string localeCode)
    {
        var (culture, _) = GetCultureAndRegion(localeCode);

        if (culture is null)
        {
            return new LocaleNotSupportedError();
        }


        var cacheKey = GetDictionaryEntriesCacheKey(appId, culture.Name);

        var result = _cacheManager.Get(cacheKey, () => InternalFetchDictionaryByAppIdAndLocaleId(appId, culture));

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Error while loading {localeCode} dictionary for app {appId}", localeCode, appId);
            _cacheManager.Remove(cacheKey);
        }

        return result;
    }

    private Result<Dictionary<string, string>>
        InternalFetchDictionaryByAppIdAndLocaleId(string appId, CultureInfo culture)
    {
        var query = _dictionaryEntryDataService.FetchActive(_ => _.AppLocaleDictionary.AppUniqueId == appId &&
                                                                 _.AppLocaleDictionary.LocaleCode == culture.Name);

        if (!query.Any())
        {
            return LocaleNotExistError.Instance;
        }

        var dictionary = query.ToDictionary(de => de.Key, de => de.Value);

        return dictionary;
    }

    public async Task<Result<AppSupportedLocaleWithDictionary>> CreateLocale(CreateLocaleRequest createRequest)
    {
        var (culture, regionInfo) = GetCultureAndRegion(createRequest.LanguageCode, createRequest.CountryCode);

        if (culture is null ||
            regionInfo is null)
        {
            return new LocaleNotSupportedError();
        }

        AppLocaleDictionary sourceLocale = null;

        if (!string.IsNullOrEmpty(createRequest.SourceLocale))
        {
            sourceLocale = _appLocaleDictionaryDataService
               .Get(ad => ad.AppUniqueId == createRequest.AppId &&
                          ad.LocaleCode == createRequest.SourceLocale);

            if (sourceLocale is null)
            {
                return SourceLocaleNotExistError.Instance;
            }
        }

        Expression<Func<AppLocaleDictionary, bool>> queryExpression;

        queryExpression = ad =>
            ad.AppUniqueId == createRequest.AppId &&
            ad.LanguageCode == createRequest.LanguageCode &&
            ad.CountryCode == createRequest.CountryCode;

        var dictionaryEntries    = new Dictionary<string, string>();
        var newDictionaryEntries = new List<DictionaryEntry>();

        var existingLocale = _appLocaleDictionaryDataService.Fetch(queryExpression)
                                                            .FirstOrDefault();

        if (existingLocale is not null)
        {
            // don't allow override if the locale is still active
            if (!existingLocale.IsDeleted)
            {
                return LocaleAlreadyExistsError.Instance;
            }

            // otherwise hard delete the existing locale to regenerate new one
            _appLocaleDictionaryDataService.DeleteOne(ad => ad.Id == existingLocale.Id,
                                                      forceHardDelete: true);
        }

        var existingLocalizationsForApp = _appLocaleDictionaryDataService
                                         .FetchActive(ad => ad.AppUniqueId == createRequest.AppId)
                                         .ToList();

        var isDefault =
            !existingLocalizationsForApp.Any(); // no app dictionary created, the first one should be default

        if (!isDefault &&
            createRequest.IsDefault &&
            createRequest.IsEnabled)
        {
            // already have locales, but this one is set to default
            // check the existing for default and set it to false
            _appLocaleDictionaryDataService.UpdateMany(ad => ad.AppUniqueId == createRequest.AppId &&
                                                             ad.IsDefault,
                                                       _ => new AppLocaleDictionary
                                                       {
                                                           IsDefault = false
                                                       });

            isDefault = true;
        }

        var isEnabled = createRequest.IsEnabled || isDefault;



        var appName = sourceLocale?.AppName ?? createRequest.AppName;
        existingLocale = new AppLocaleDictionary
        {
            AppUniqueId  = createRequest.AppId,
            AppName      = appName,
            Description  = $"Translation dictionary for {appName} in {culture.EnglishName} language",
            LanguageCode = culture.TwoLetterISOLanguageName,
            CountryCode  = regionInfo.TwoLetterISORegionName,
            LocaleCode   = culture.Name,
            DisplayName  = culture.DisplayName,
            IsActive     = isEnabled,
            IsDefault    = isDefault
        };

        await _appLocaleDictionaryDataService.InsertAsync(existingLocale);

        if (!string.IsNullOrEmpty(createRequest.SourceLocale) &&
            sourceLocale is not null)
        {
            var sourceDictionaryEntries =
                _dictionaryEntryDataService.FetchActive(de => de.DictionaryId == sourceLocale.Id)
                                           .ToList();

            foreach (var sourceDictionaryEntry in sourceDictionaryEntries)
            {
                var newDictionaryEntry = new DictionaryEntry
                {
                    DictionaryId = existingLocale.Id,
                    Key          = sourceDictionaryEntry.Key,
                    Value        = sourceDictionaryEntry.Value,
                    Description  = sourceDictionaryEntry.Description
                };
                newDictionaryEntries.Add(newDictionaryEntry);

                dictionaryEntries.Add(sourceDictionaryEntry.Key, sourceDictionaryEntry.Value);
            }
        }

        if (newDictionaryEntries.Any())
        {
            await _dictionaryEntryDataService.InsertAsync(newDictionaryEntries);
        }

        var cacheKey = GetAppSupportedLocalesCacheKey(createRequest.AppId);
        _cacheManager.Remove(cacheKey);

        return new AppSupportedLocaleWithDictionary
        {
            Id                = existingLocale.Id,
            AppName           = existingLocale.AppName,
            AppUniqueId       = existingLocale.AppUniqueId,
            CountryCode       = existingLocale.CountryCode,
            DisplayName       = existingLocale.DisplayName,
            Description       = existingLocale.Description,
            IsDefault         = existingLocale.IsDefault,
            LanguageCode      = existingLocale.LanguageCode,
            LocaleCode        = existingLocale.LocaleCode,
            LanguageName      = culture.EnglishName,
            CountryName       = regionInfo.EnglishName,
            DictionaryEntries = dictionaryEntries
        };
    }

    public async Task<Result<bool>> DeleteDictionaryEntry(long entryId,
                                                          bool alsoDeleteFromOtherDictionaries = true)
    {
        var entry = _dictionaryEntryDataService.Get(de => de.Id == entryId);

        if (entry is null)
            return DictionaryEntryNotFoundError.Instance;

        if (alsoDeleteFromOtherDictionaries)
        {
            var otherDictionariesOfSameApp = _dictionaryEntryDataService
                                            .Fetch(de => de.AppLocaleDictionary.AppUniqueId ==
                                                         entry.AppLocaleDictionary.AppUniqueId &&
                                                         de.Key == entry.Key)
                                            .Select(e => e.DictionaryId)
                                            .ToList();

            _dictionaryEntryDataService.DeleteMany(de => de.Key == entry.Key && (de.Id == entryId ||
                                                                                 otherDictionariesOfSameApp
                                                                                    .Contains(de.DictionaryId)));
        }
        else
        {
            _dictionaryEntryDataService.DeleteOne(de => de.Id == entryId);
        }

        return true;
    }

    public async Task<Result<Dictionary<string, string>>> UpsertDictionaryEntries(
        DictionaryEntriesImportRequest importRequest)
    {
        if (importRequest.DictionaryId == 0)
        {
            return DictionaryMustBeSpecifiedError.Instance;
        }

        var importingDicEntryQuery = _appLocaleDictionaryDataService.Get(de => de.Id == importRequest.DictionaryId);

        if (importingDicEntryQuery is null)
            return DictionaryEntryNotFoundError.Instance;

        var dictionariesToCreateEntriesIn = new List<long>
        {
            importingDicEntryQuery.Id
        };

        var existingDictionary = await GetDictionaryEntriesByLocale(importingDicEntryQuery.AppUniqueId,
                                                                    importingDicEntryQuery.LocaleCode);

        var importingEntryKeys = importRequest.Entries.Keys.ToList();

        var existingEntryKeys = existingDictionary.Value?.Keys.ToList() ?? [];

        // get the new keys that are not existed in the dictionary
        var newEntryKeys = importingEntryKeys.Except(existingEntryKeys)
                                             .ToList();

        if (importRequest.OverrideExistingValuesInOtherDictionaries)
        {
            // should only apply to the keys that are not existed in the other dictionaries
            var otherAppLocales = _dictionaryEntryDataService
                                 .Fetch(de => de.AppLocaleDictionary.AppUniqueId ==
                                              importingDicEntryQuery.AppUniqueId &&
                                              !newEntryKeys.Contains(de.Key))
                                 .Select(de => de.DictionaryId)
                                 .ToList();

            dictionariesToCreateEntriesIn.AddRange(otherAppLocales);
        }

        var newEntries = importRequest.Entries
                                      .Where(e => newEntryKeys.Contains(e.Key));

        Queue<Func<Task>> tasksToExecutes = [];

        var entriesToCreate = dictionariesToCreateEntriesIn
                             .SelectMany(id =>
                              {

                                  return newEntries.Select(entry => new DictionaryEntry
                                  {
                                      DictionaryId = id,
                                      Key          = entry.Key,
                                      Value        = entry.Value
                                  });
                              })
                             .ToList();

        tasksToExecutes.Enqueue(async () =>
        {
            _logger.LogInformation("Creating {numbersOfEntries} dictionary entries", entriesToCreate.Count);

            await _dictionaryEntryDataService.InsertAsync(entriesToCreate);

            _logger.LogInformation("Created {numbersOfEntries} dictionary entries", entriesToCreate.Count);
        });

        var existingEntries = importRequest.Entries
                                           .Where(e => existingEntryKeys.Contains(e.Key));

        foreach (var entry in existingEntries)
        {
            var updatedValue = entry.Value;
            var entryKey     = entry.Key;
            var dictionaryId = importingDicEntryQuery.Id;

            tasksToExecutes.Enqueue(async () =>
            {
                _dictionaryEntryDataService.UpdateOne(de => de.DictionaryId == dictionaryId &&
                                                            de.Key == entryKey,
                                                      _ => new DictionaryEntry
                                                      {
                                                          Value = updatedValue
                                                      });
            });
        }

        try
        {
            while (tasksToExecutes.Any())
            {
                var tasksToExecute = tasksToExecutes.Dequeue();

                try
                {
                    await tasksToExecute();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Task execution failed, break transaction");

                    throw;
                }
            }

            return importRequest.Entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating dictionary entries");

            throw;
        }
        finally
        {
            // remove the cache for next fetch
            var cacheKey =
                GetDictionaryEntriesCacheKey(importingDicEntryQuery.AppUniqueId, importingDicEntryQuery.LocaleCode);
            _cacheManager.Remove(cacheKey);
        }
    }

    private static CacheKey GetSystemSupportedLocalesCacheKey(string[] countryCodes)
        => new("SystemSupportedLocales", cacheTime: 2, countryCodes);

    private static CacheKey GetAppSupportedLocalesCacheKey(string appId)
        => new($"AppLocales_{appId}", cacheTime: 2);

    private static CacheKey GetDictionaryEntriesCacheKey(string appId, string localeCode)
        => new($"DictionaryEntries_{appId}_{localeCode}", cacheTime: 15);

    // Mark as internal for testing purposes
    internal static List<SupportedLocale> InternalGetSystemSupportedLocales(params string[] countryCodes)
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        if (countryCodes.Any())
        {
            cultures = cultures
                      .Where(c => countryCodes.Any(cc => c.Name.EndsWith(cc, StringComparison.OrdinalIgnoreCase)))
                      .ToArray();
        }

        var supportedLocales = new List<SupportedLocale>();

        foreach (var culture in cultures)
        {
            try
            {
                var region = new RegionInfo(culture.Name);

                supportedLocales.Add(new SupportedLocale
                {
                    LanguageCode = culture.TwoLetterISOLanguageName,
                    CountryCode  = region.TwoLetterISORegionName,
                    LanguageName = culture.EnglishName,
                    DisplayName  = culture.DisplayName,
                    CountryName  = region.DisplayName,
                    LocaleCode   = culture.Name
                });
            }
            catch (ArgumentException)
            {
                // ignored
            }
        }

        var orderedResult = supportedLocales.OrderBy(_ => _.DisplayName)
                                            .ToList();

        return orderedResult;
    }

    private List<SupportedLocale> InternalGetSupportedLocalesByAppId(string appId)
    {
        var query = _appLocaleDictionaryDataService.FetchActive(_ => _.AppUniqueId == appId)
                                                   .ToList();

        var supportedLocales = new List<SupportedLocale>();

        foreach (var appLocaleDictionary in query)
        {
            var (culture, regionInfo) = GetCultureAndRegion(appLocaleDictionary.LocaleCode);

            if (culture is null ||
                regionInfo is null)
            {
                // clean up the unsupported locales from database
                _dictionaryEntryDataService.DeleteMany(_ => _.DictionaryId == appLocaleDictionary.Id,
                                                       forceHardDelete: true);

                _appLocaleDictionaryDataService.DeleteOne(ad => ad.Id == appLocaleDictionary.Id, forceHardDelete: true);

                continue;
            }

            supportedLocales.Add(new SupportedLocale
            {
                Id           = appLocaleDictionary.Id,
                LanguageCode = culture.TwoLetterISOLanguageName,
                CountryCode  = regionInfo.TwoLetterISORegionName,
                LanguageName = culture.EnglishName,
                DisplayName  = culture.DisplayName,
                CountryName  = regionInfo.DisplayName,
                LocaleCode   = culture.Name,
                IsDefault    = appLocaleDictionary.IsDefault
            });
        }

        return supportedLocales;
    }

    private static (CultureInfo, RegionInfo) GetCultureAndRegion(string cultureName)
    {
        var splitStrings = cultureName.Split(new[]
                                             {
                                                 '-', '_'
                                             },
                                             StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (splitStrings.Length != 2)
        {
            throw new
                ArgumentException("The provided culture id is not valid. It must in format {localeCode}_{countryCode} or {localeCode}-{countryCode}",
                                  nameof(cultureName));
        }

        return GetCultureAndRegion(splitStrings[0], splitStrings[1]);
    }

    private static (CultureInfo, RegionInfo) GetCultureAndRegion(string localeCode, string countryCode)
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        CultureInfo culture =
            cultures.FirstOrDefault(c => c.Name.Equals($"{localeCode}_{countryCode}",
                                                       StringComparison.OrdinalIgnoreCase)) ??
            cultures.FirstOrDefault(c => c.Name.Equals($"{localeCode}-{countryCode}",
                                                       StringComparison.OrdinalIgnoreCase));

        if (culture is null)
        {
            return (default, default);
        }

        var regionInfo = new RegionInfo(culture.Name);

        return (culture, regionInfo);
    }
}