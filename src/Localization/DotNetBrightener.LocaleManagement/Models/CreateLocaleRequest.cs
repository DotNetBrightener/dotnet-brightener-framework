namespace DotNetBrightener.LocaleManagement.Models;

/// <summary>
///     Represents the request model for creating a new locale
/// </summary>
public class CreateLocaleRequest
{
    /// <summary>
    ///     Unique Id of the app
    /// </summary>
    public string AppId { get; init; }

    public string  AppName      { get; init; }
    public string  LanguageCode { get; init; }
    public string  CountryCode  { get; init; }
    public string? SourceLocale { get; init; }
}