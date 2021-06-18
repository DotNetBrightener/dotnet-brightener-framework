using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DotNetBrightener.Caching;

namespace DotNetBrightener.Core.Localization.Services
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);

        void ClearDictionaryCache();
    }

    public class LocalizationManager : ILocalizationManager
    {
        private static readonly PluralizationRuleDelegate DefaultPluralRule = n => n != 1 ? 1 : 0;

        private const    string                                CacheKeyPrefix = "CultureDictionary-";
        private readonly ICacheProvider                   _cache;
        private readonly IEnumerable<ILocalizationFileManager>  _localizationFileLoader;
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public LocalizationManager(ICacheProvider                  cache,
                                   IEnumerable<ILocalizationFileManager> localizationFileLoader)
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

        public void ClearDictionaryCache()
        {
            _cache.RemoveByPrefix(CacheKeyPrefix);
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
            var localizationFiles = _localizationFileLoader.SelectMany(_ => _.LoadTranslationFiles(cultureName))
                                                           .Where(_ => _.Exists)
                                                           .ToArray();

            foreach (var localizationFile in localizationFiles)
            {
                LoadFileToDictionary(localizationFile, dictionary, cultureName);
            }
        }

        private void LoadFileToDictionary(ITranslationFileInfo fileInfo, CultureDictionary dictionary, string cultureName)
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

                var content     = fileInfo.GetDictionaryEntries();

                var dict = content.Where(_ => !string.IsNullOrEmpty(_.Key))
                                  .Select(_ => new CultureDictionaryRecord(_.Key, _.Value));

                dictionary.MergeTranslations(dict);
            }
        }

        private FileSystemWatcher PrepareFileWatcher(string containingFolder, string cultureName)
        {
            var fileWatcher = new FileSystemWatcher(containingFolder);

            fileWatcher.Changed += (sender, args) =>
            {
                _cache.RemoveByPrefix(CacheKeyPrefix + cultureName);
            };

            fileWatcher.EnableRaisingEvents = true;

            return fileWatcher;
        }


        private static CacheKey GetCacheKey(CultureInfo culture)
        {
            return new CacheKey(CacheKeyPrefix + culture.TwoLetterISOLanguageName, 10, 
                                CacheKeyPrefix,
                                CacheKeyPrefix + culture.TwoLetterISOLanguageName);
        }
    }
}