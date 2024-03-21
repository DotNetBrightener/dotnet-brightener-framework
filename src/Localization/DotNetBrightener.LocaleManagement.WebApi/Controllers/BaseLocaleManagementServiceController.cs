using DotNetBrightener.LocaleManagement.Data;
using DotNetBrightener.LocaleManagement.Entities;
using DotNetBrightener.LocaleManagement.Models;
using DotNetBrightener.LocaleManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetBrightener.LocaleManagement.WebApi.Controllers;

public abstract class BaseLocaleManagementServiceController : Controller
{
    protected readonly ILocaleManagementService        LocaleManagementService;
    protected readonly IAppLocaleDictionaryDataService AppLocaleDictionaryDataService;
    protected readonly IDictionaryEntryDataService     DictionaryEntryDataService;
    protected readonly ILogger                         Logger;

    protected BaseLocaleManagementServiceController(ILocaleManagementService        localeManagementService,
                                                    IAppLocaleDictionaryDataService appLocaleDictionaryDataService,
                                                    IDictionaryEntryDataService     dictionaryEntryDataService,
                                                    ILogger                         logger)
    {
        LocaleManagementService        = localeManagementService;
        AppLocaleDictionaryDataService = appLocaleDictionaryDataService;
        DictionaryEntryDataService     = dictionaryEntryDataService;


        Logger = logger;
    }

    [HttpGet("~/api/localeManagement/supportedLocales")]
    public virtual async Task<IActionResult> GetSupportedLocales()
    {
        var isAuthorized = await CanRetrieveLocales();

        if (!isAuthorized)
        {
            return StatusCode(404);
        }

        var locales = await LocaleManagementService.GetSystemSupportedLocales();

        return Ok(locales);
    }


    /// <summary>
    ///     Retrieves the localization for all apps
    /// </summary>
    /// <returns></returns>
    [HttpGet("~/api/localeManagement")]
    public virtual async Task<IActionResult> GetAllAppLocales()
    {
        var allAppIds = AppLocaleDictionaryDataService.Fetch()
                                                      .Select(ad => new AppLocaleDictionary
                                                       {
                                                           AppUniqueId = ad.AppUniqueId,
                                                           AppName     = ad.AppName
                                                       })
                                                      .Distinct()
                                                      .ToList();

        List<AppSupportedLocale> appLocales = allAppIds
                                             .SelectMany(app =>
                                              {
                                                  var appSupportedLocale = LocaleManagementService
                                                                          .GetSupportedLocalesByAppId(app.AppUniqueId)
                                                                          .Result;

                                                  return appSupportedLocale.Select(locale => new AppSupportedLocale
                                                  {
                                                      Id           = locale.Id,
                                                      AppName      = app.AppName,
                                                      AppUniqueId  = app.AppUniqueId,
                                                      CountryCode  = locale.CountryCode,
                                                      CountryName  = locale.CountryName,
                                                      DisplayName  = locale.DisplayName,
                                                      LanguageCode = locale.LanguageCode,
                                                      LanguageName = locale.LanguageName,
                                                      LocaleCode   = locale.LocaleCode,
                                                      IsDefault    = locale.IsDefault
                                                  });
                                              })
                                             .ToList();

        return Ok(appLocales);
    }


    /// <summary>
    ///     Get dictionary with info and entries for the given localized dictionary
    /// </summary>
    /// <param name="localizationId">The identifier of the dictionary / localization</param>
    /// <returns></returns>
    [HttpGet("~/api/localeManagement/{localizationId}")]
    public virtual async Task<IActionResult> GetDictionaryWithInfo(long localizationId)
    {
        var info = AppLocaleDictionaryDataService.Get(ad => ad.Id == localizationId);

        if (info is null)
        {
            return StatusCode((int)HttpStatusCode.NotFound);
        }

        var dictionaryEntries = DictionaryEntryDataService.Fetch(entry => entry.DictionaryId == localizationId)
                                                          .ToList();

        var localeInfos = await LocaleManagementService.GetSupportedLocalesByAppId(info.AppUniqueId);

        var localeInfo = localeInfos.FirstOrDefault(_ => _.Id == info.Id);

        return Ok(new AppSupportedLocaleWithDictionary
        {
            Id                = info.Id,
            AppName           = info.AppName,
            AppUniqueId       = info.AppUniqueId,
            CountryCode       = info.CountryCode,
            DisplayName       = info.DisplayName,
            CountryName       = localeInfo!.CountryName,
            LanguageName      = localeInfo.LanguageName,
            LanguageCode      = localeInfo.LanguageCode,
            LocaleCode        = localeInfo.LocaleCode,
            IsDefault         = localeInfo.IsDefault,
            DictionaryEntries = dictionaryEntries.ToDictionary(entry => entry.Key, entry => entry.Value),
        });
    }

    [HttpGet("~/api/localeManagement/{appId}/supportedLocales")]
    public virtual async Task<IActionResult> GetSupportedLocalesByAppId(string appId)
    {
        var isAuthorized = await CanRetrieveLocales(appId);

        if (!isAuthorized)
        {
            return StatusCode(404);
        }

        var locales = await LocaleManagementService.GetSupportedLocalesByAppId(appId);

        return Ok(locales);
    }

    [HttpGet("~/api/localeManagement/{appId}/dictionary/{languageCode}/{countryCode}")]
    public virtual async Task<IActionResult> GetDictionaryByAppAndLocale(string appId,
                                                                         string languageCode,
                                                                         string countryCode)
    {
        var isAuthorized = await CanRetrieveDictionary(appId);

        if (!isAuthorized)
        {
            return StatusCode(404);
        }

        var dictionaryEntries =
            await LocaleManagementService.GetDictionaryEntriesByLocale(appId,
                                                                       $"{languageCode}_{countryCode}");

        return dictionaryEntries.Match(Ok,
                                       error => StatusCode(error.StatusCode, error));
    }

    [HttpPost("~/api/localeManagement/appLocaleDictionary")]
    public virtual async Task<IActionResult> CreateLocale([FromBody] CreateLocaleRequest request)
    {
        var canUpdate = await CanCreateDictionary();

        if (!canUpdate)
        {
            return StatusCode(403);
        }

        var localeData = await LocaleManagementService.CreateLocale(request);

        return localeData.Match(Ok,
                                error => StatusCode(error.StatusCode, error));
    }

    [HttpPost("~/api/localeManagement/appLocaleDictionary/{localizationId}/import")]
    public virtual async Task<IActionResult> ImportLocale(long localizationId,
                                                          [FromForm]
                                                          DictionaryEntriesImportRequest request)
    {
        var canUpdate = await CanCreateDictionary();

        if (!canUpdate)
        {
            return StatusCode(403);
        }

        if (Request.Form.Files.Count == 0)
        {
            return StatusCode(403,
                              new
                              {
                                  ErrorMessage = "No files were found for import."
                              });
        }

        var dictionaryEntries = new Dictionary<string, string>();

        foreach (var formFile in Request.Form.Files)
        {
            using var reader = new StreamReader(formFile.OpenReadStream());

            try
            {
                var fileContent   = await reader.ReadToEndAsync();
                var entries       = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
                var entriesInFile = entries ?? new Dictionary<string, string>();

                if (dictionaryEntries.Count > 0 &&
                    dictionaryEntries.Keys.Intersect(entriesInFile.Keys).Any())
                {
                    throw new Exception("The dictionary entry key is duplicated in the request data");
                }

                dictionaryEntries = dictionaryEntries.Concat(entriesInFile)
                                                     .ToDictionary(k => k.Key, v => v.Value);
            }
            catch (JsonException ex)
            {
                Logger.LogWarning(ex, "Error while parsing dictionary file for importing");

                return StatusCode(403,
                                  new
                                  {
                                      ErrorMessage = "The file is not in JSON dictionary format."
                                  });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while reading files for importing");

                return StatusCode(403,
                                  new
                                  {
                                      ErrorMessage = ex.GetFullExceptionMessage()
                                  });
            }
        }

        var localeData = await LocaleManagementService.UpsertDictionaryEntries(new DictionaryEntriesImportRequest
        {
            DictionaryId                              = localizationId,
            Entries                                   = dictionaryEntries,
            OverrideExistingValuesInOtherDictionaries = request.OverrideExistingValuesInOtherDictionaries
        });

        return localeData.Match(Ok,
                                error => StatusCode(error.StatusCode, error));
    }

    [HttpPost("~/api/localeManagement/appLocaleDictionary/{localizationId}/entries")]
    public virtual async Task<IActionResult> SaveDictionaryEntries(long localizationId,
                                                                   [FromBody]
                                                                   DictionaryEntriesImportRequest request)
    {

        var canUpdate = await CanUpdateDictionary();

        if (!canUpdate)
        {
            return StatusCode(403);
        }

        var localeData = await LocaleManagementService.UpsertDictionaryEntries(new DictionaryEntriesImportRequest
        {
            DictionaryId                              = localizationId,
            Entries                                   = request.Entries,
            OverrideExistingValuesInOtherDictionaries = request.OverrideExistingValuesInOtherDictionaries
        });

        return localeData.Match(Ok,
                                error => StatusCode(error.StatusCode, error));
    }


    [HttpDelete("~/api/localeManagement/entry/{entryId}")]
    public virtual async Task<IActionResult> DeleteDictionaryEntry(long entryId)
    {
        var canUpdate = await CanDeleteDictionaryEntry();

        if (!canUpdate)
        {
            return StatusCode(403);
        }

        var result = await LocaleManagementService.DeleteDictionaryEntry(entryId);

        return result.Match<IActionResult>(_ => Ok(),
                                           error => StatusCode(error.StatusCode, error));
    }

    [NonAction]
    public abstract Task<bool> CanRetrieveDictionary(string appId = null);

    [NonAction]
    public abstract Task<bool> CanRetrieveLocales(string appId = null);

    [NonAction]
    public abstract Task<bool> CanCreateDictionary();

    [NonAction]
    public abstract Task<bool> CanDeleteDictionary();

    [NonAction]
    public abstract Task<bool> CanUpdateDictionary();

    [NonAction]
    public abstract Task<bool> CanDeleteDictionaryEntry();
}