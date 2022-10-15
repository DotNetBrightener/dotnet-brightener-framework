using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Core.Localization;

/// <summary>
///     Represents a modular collection of <see cref="TranslationEntry"/>
/// </summary>
public class TranslationDictionary
{
    /// <summary>
    ///     Represents the identifier of the module if the modular system is available
    /// </summary>
    public string ModuleId { get; set; }

    /// <summary>
    ///     The name of the module if the modular system is available
    /// </summary>
    public string ModuleName { get; set; }

    /// <summary>
    ///     The language of the translation
    /// </summary>
    public string LocaleName { get; set; }

    /// <summary>
    ///     The translation entries
    /// </summary>
    public IEnumerable<TranslationEntry> Translations { get; set; }

    /// <summary>
    ///     Convert the TranslationEntry to Dictionary
    /// </summary>
    /// <returns></returns>
    public CultureDictionaryJsonFormat ToDictionaryFormat()
    {
        return new CultureDictionaryJsonFormat(Translations.ToDictionary(_ => _.Key, _ => _.Value));
    }
}