using DotNetBrightener.LocaleManagement.Models;
using DotNetBrightener.LocaleManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBrightener.LocaleManagement.WebApi.Controllers;

public abstract class BaseLocaleManagementServiceController : Controller
{
    protected readonly ILocaleManagementService LocaleManagementService;
    protected readonly ILogger                  Logger;

    protected BaseLocaleManagementServiceController(ILocaleManagementService                   localeManagementService,
                                                ILogger<BaseLocaleManagementServiceController> logger)
    {
        LocaleManagementService = localeManagementService;
        Logger                  = logger;
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

    [HttpPost("~/api/localeManagement/appLocaleDictionary/{dictionaryId}/import")]
    public virtual async Task<IActionResult> ImportLocale(long dictionaryId,
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
            DictionaryId                              = dictionaryId,
            Entries                                   = dictionaryEntries,
            OverrideExistingValuesInOtherDictionaries = request.OverrideExistingValuesInOtherDictionaries
        });

        return localeData.Match(Ok,
                                error => StatusCode(error.StatusCode, error));
    }

    [HttpPost("~/api/localeManagement/appLocaleDictionary/{dictionaryId}/entries")]
    public virtual async Task<IActionResult> SaveDictionaryEntries(long                                    dictionaryId, 
                                                                   [FromBody] DictionaryEntriesImportRequest request)
    {

        var canUpdate = await CanUpdateDictionary();

        if (!canUpdate)
        {
            return StatusCode(403);
        }

        var localeData = await LocaleManagementService.UpsertDictionaryEntries(new DictionaryEntriesImportRequest
        {
            DictionaryId                              = dictionaryId,
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