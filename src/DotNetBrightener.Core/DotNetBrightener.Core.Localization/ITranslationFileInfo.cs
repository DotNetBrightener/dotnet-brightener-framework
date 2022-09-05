using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.Localization;

/// <summary>
///     Represents the translation file information
/// </summary>
public interface ITranslationFileInfo: IFileInfo
{
    /// <summary>
    ///     The module Id associated with the translation
    /// </summary>
    string ModuleId { get; set; }

    /// <summary>
    ///     The module name associated with the translation file
    /// </summary>
    string ModuleName { get; set; }

    /// <summary>
    ///     The translation language
    /// </summary>
    string LocaleName { get; set; }

    /// <summary>
    ///     Retrieves the dictionary for the translation
    /// </summary>
    /// <returns></returns>
    TranslationDictionary RetrieveDictionary();

    /// <summary>
    ///     Load the dictionary entries from the translation
    /// </summary>
    /// <returns></returns>
    CultureDictionaryJsonFormat GetDictionaryEntries();
}