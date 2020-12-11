using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace DotNetBrightener.Core.Localization
{
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

    public class TranslationFileInfo : PhysicalFileInfo, ITranslationFileInfo
    {
        private Lazy<TranslationDictionary> _lazyTranslationLoader;
        private Lazy<CultureDictionaryJsonFormat> _translationEntries;

        public TranslationFileInfo(FileInfo info) : base(info)
        {
            _lazyTranslationLoader = new Lazy<TranslationDictionary>(() => InternalRetrieveDictionary());
            _translationEntries = new Lazy<CultureDictionaryJsonFormat>(() => InternalLoadDictionaryEntries());
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
                ModuleId = ModuleId,
                ModuleName = ModuleName,
                LocaleName = LocaleName,
                Translations = translation.Select(_ => new TranslationEntry
                {
                    Key = _.Key,
                    Value = _.Value
                })
            };
        }
    }
}