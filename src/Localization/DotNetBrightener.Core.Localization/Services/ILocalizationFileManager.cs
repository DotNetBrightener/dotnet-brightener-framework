namespace DotNetBrightener.Core.Localization.Services;

/// <summary>
///     The service provides functions for managing localization files
/// </summary>
public interface ILocalizationFileManager
{
    /// <summary>
    ///     Loads the localization files for the given language
    /// </summary>
    /// <param name="cultureName">The culture name of the language</param>
    /// <returns>
    ///     An enumerable of <see cref="ITranslationFileInfo"/>
    /// </returns>
    IEnumerable<ITranslationFileInfo> LoadTranslationFiles(string cultureName);

    /// <summary>
    ///     Saves the translations asynchronously
    /// </summary>
    /// <param name="translationDictionary">The dictionary of translation entries to save</param>
    Task SaveTranslationsAsync(TranslationDictionary translationDictionary);

    /// <summary>
    ///     Saves the translations
    /// </summary>
    /// <param name="translationDictionary">The dictionary of translation entries to save</param>
    void SaveTranslations(TranslationDictionary translationDictionary);
}