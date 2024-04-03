using Microsoft.Extensions.FileProviders.Physical;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Localization;

public class TranslationFileInfo : PhysicalFileInfo, ITranslationFileInfo
{
    private Lazy<TranslationDictionary>       _lazyTranslationLoader;
    private Lazy<CultureDictionaryJsonFormat> _translationEntries;

    public TranslationFileInfo(FileInfo info) : base(info)
    {
        _lazyTranslationLoader = new Lazy<TranslationDictionary>(InternalRetrieveDictionary);
        _translationEntries    = new Lazy<CultureDictionaryJsonFormat>(InternalLoadDictionaryEntries);
    }

    public string ModuleId { get; set; }

    public string ModuleName { get; set; }

    public string LocaleName { get; set; }

    public TranslationDictionary RetrieveDictionary()
    {
        return _lazyTranslationLoader.Value;
    }

    public CultureDictionaryJsonFormat GetDictionaryEntries()
    {
        return _translationEntries.Value;
    }

    private CultureDictionaryJsonFormat InternalLoadDictionaryEntries()
    {
        var translation = new CultureDictionaryJsonFormat();

        if (File.Exists(PhysicalPath))
        {
            var fileContent = File.ReadAllText(PhysicalPath);
            translation = JsonConvert.DeserializeObject<CultureDictionaryJsonFormat>(fileContent);
        }

        return translation;
    }

    private TranslationDictionary InternalRetrieveDictionary()
    {
        var translation = _translationEntries.Value;

        return new TranslationDictionary
        {
            ModuleId   = ModuleId,
            ModuleName = ModuleName,
            LocaleName = LocaleName,
            Translations = translation.Select(_ => new TranslationEntry
            {
                Key   = _.Key,
                Value = _.Value
            })
        };
    }
}