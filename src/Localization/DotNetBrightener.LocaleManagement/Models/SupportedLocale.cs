using System.Collections.Generic;
using LocaleManagement.Entities;

namespace LocaleManagement.Models;

public class SupportedLocale
{
    public long?  Id           { get; set; }
    public string LanguageCode { get; set; }
    public string CountryCode  { get; set; }
    public string DisplayName  { get; set; }
    public string LanguageName { get; set; }
    public string CountryName  { get; set; }
    public string LocaleCode   { get; set; }
    public string Description  { get; set; }
    public bool   IsDefault    { get; set; }
}


public class AppSupportedLocale : SupportedLocale
{
    public string AppUniqueId { get; set; }

    public string AppName { get; set; }

    public static AppSupportedLocale FromAppDictionary(AppLocaleDictionary localizedDictionary)
    {
        return new AppSupportedLocale
        {
            Id           = localizedDictionary.Id,
            AppName      = localizedDictionary.AppName,
            AppUniqueId  = localizedDictionary.AppUniqueId,
            CountryCode  = localizedDictionary.CountryCode,
            DisplayName  = localizedDictionary.DisplayName,
            Description  = localizedDictionary.Description,
            IsDefault    = localizedDictionary.IsDefault,
            LanguageCode = localizedDictionary.LanguageCode,
            LocaleCode   = localizedDictionary.LocaleCode
        };
    }
}

public class AppSupportedLocaleWithDictionary : AppSupportedLocale
{
    public Dictionary<string, string> DictionaryEntries { get; set; }
}