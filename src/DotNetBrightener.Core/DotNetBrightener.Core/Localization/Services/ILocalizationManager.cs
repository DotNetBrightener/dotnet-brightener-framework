using System.Globalization;

namespace DotNetBrightener.Core.Localization.Services;

public interface ILocalizationManager
{
    CultureDictionary GetDictionary(CultureInfo culture);

    void ClearDictionaryCache();
}