using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DotNetBrightener.Core.Caching;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Localization.Services
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);
    }

    public class LocalizationManager : ILocalizationManager
    {
        private static readonly PluralizationRuleDelegate DefaultPluralRule = n => (n != 1 ? 1 : 0);

        private const    string                                CacheKeyPrefix = "CultureDictionary-";
        private readonly IStaticCacheManager                   _cache;
        private readonly IEnumerable<ILocalizationFileLoader>  _localizationFileLoader;
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public LocalizationManager(IStaticCacheManager                  cache,
                                   IEnumerable<ILocalizationFileLoader> localizationFileLoader)
        {
            _cache                  = cache;
            _localizationFileLoader = localizationFileLoader;
        }

        public CultureDictionary GetDictionary(CultureInfo culture)
        {
            var cacheKey         = GetCacheKey(culture);
            var cachedDictionary = _cache.Get(cacheKey, () => CreateDictionary(culture));

            return cachedDictionary;
        }

        private CultureDictionary CreateDictionary(CultureInfo culture)
        {
            PluralizationRuleDelegate rule = DefaultPluralRule;

            var dictionary = new CultureDictionary(culture.TwoLetterISOLanguageName, rule);
            LoadTranslations(culture.TwoLetterISOLanguageName, dictionary);

            return dictionary;
        }

        private void LoadTranslations(string cultureName, CultureDictionary dictionary)
        {
            var localizationFiles = _localizationFileLoader.SelectMany(_ => _.LoadTranslations(cultureName))
                                                           .Where(_ => _.Exists)
                                                           .ToArray();

            foreach (var localizationFile in localizationFiles)
            {
                LoadFileToDictionary(localizationFile, dictionary, cultureName);
            }
        }

        private void LoadFileToDictionary(IFileInfo fileInfo, CultureDictionary dictionary, string cultureName)
        {
            if (fileInfo.Exists)
            {
                var containingFolder = Path.GetDirectoryName(fileInfo.PhysicalPath);
                var watcherKey       = $"{containingFolder}|||WATCHER|||{cultureName}";
                if (!_watchers.TryGetValue(watcherKey, out var fileWatcher))
                {
                    fileWatcher = PrepareFileWatcher(containingFolder, cultureName);
                    _watchers.Add(watcherKey, fileWatcher);
                }

                var fileContent = File.ReadAllText(fileInfo.PhysicalPath);
                var content     = JsonConvert.DeserializeObject<CultureDictionaryJsonFormat>(fileContent);

                var dict = content.Select(_ => new CultureDictionaryRecord(_.Key, _.Value));

                dictionary.MergeTranslations(dict);
            }
        }

        private FileSystemWatcher PrepareFileWatcher(string containingFolder, string cultureName)
        {
            var fileWatcher = new FileSystemWatcher(containingFolder);

            fileWatcher.Changed += (sender, args) =>
            {
                _cache.Remove(new CacheKey(CacheKeyPrefix + cultureName));
            };

            fileWatcher.EnableRaisingEvents = true;

            return fileWatcher;
        }


        private static CacheKey GetCacheKey(CultureInfo culture)
        {
            return new CacheKey(CacheKeyPrefix + culture.TwoLetterISOLanguageName, 10);
        }
    }
}