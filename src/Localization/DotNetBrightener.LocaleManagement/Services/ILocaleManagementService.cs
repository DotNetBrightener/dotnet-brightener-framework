﻿using LanguageExts.Results;
using LocaleManagement.Models;

// ReSharper disable JoinDeclarationAndInitializer

namespace LocaleManagement.Services;

public interface ILocaleManagementService
{
    /// <summary>
    ///     Retrieves the supported locales by the system
    /// </summary>
    /// <param name="countryCodes">Optionally specified countries codes to retrieve the locales for</param>
    /// <returns>Collection of supported locales</returns>
    Task<List<SupportedLocale>> GetSystemSupportedLocales(params string[] countryCodes);

    /// <summary>
    ///     Retrieves the supported locales configured for the given app
    /// </summary>
    /// <param name="appId">Unique id of the app to get the supported locales</param>
    /// <returns>Collection of supported locales configured for the specified app</returns>
    Task<List<SupportedLocale>> GetSupportedLocalesByAppId(string appId);

    /// <summary>
    ///     Retrieves the dictionary entries for the specified locale and app
    /// </summary>
    /// <param name="appId">Unique id of the app</param>
    /// <param name="localeCode">The code of the locale to get the dictionary entries</param>
    /// <returns></returns>
    Task<Result<Dictionary<string, string>>> GetDictionaryEntriesByLocale(string appId, string localeCode);

    /// <summary>
    ///     Creates a new locale with the specified request
    /// </summary>
    /// <param name="createRequest"></param>
    /// <returns></returns>
    Task<Result<AppSupportedLocaleWithDictionary>> CreateLocale(CreateLocaleRequest createRequest);

    /// <summary>
    ///     Deletes a specified dictionary entry
    /// </summary>
    /// <param name="entryId">The identifier of the dictionary entry</param>
    /// <param name="alsoDeleteFromOtherDictionaries"></param>
    /// <returns></returns>
    Task<Result<bool>> DeleteDictionaryEntry(long entryId,
                                             bool alsoDeleteFromOtherDictionaries = true);

    /// <summary>
    ///     Insert or Update the given entries to the specified dictionary
    /// </summary>
    /// <param name="importRequest">The import request contains the translated entries</param>
    /// <returns></returns>
    Task<Result<Dictionary<string, string>>> UpsertDictionaryEntries(DictionaryEntriesImportRequest importRequest);
}