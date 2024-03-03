namespace DotNetBrightener.LocaleManagement.Models;

public class SupportedLocale
{
    public long?  Id           { get; set; }
    public string LanguageCode { get; set; }
    public string CountryCode  { get; set; }
    public string DisplayName  { get; set; }
    public string LanguageName { get; set; }
    public string CountryName  { get; set; }
    public string LocaleName   { get; set; }
}