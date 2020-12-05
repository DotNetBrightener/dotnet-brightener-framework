using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Localization.Services
{
    public interface ILocalizationFileManager
    {
        IEnumerable<ITranslationFileInfo> LoadTranslationFiles(string cultureName);

        Task SaveTranslationsAsync(TranslationDictionary translationDictionary);

        void SaveTranslations(TranslationDictionary translationDictionary);
    }

    public class DefaultLocalizationFileLoader : ILocalizationFileManager
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DefaultLocalizationFileLoader(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public virtual IEnumerable<ITranslationFileInfo> LoadTranslationFiles(string cultureName)
        {
            var localeFile = Path.Combine(_webHostEnvironment.ContentRootPath, "Locales", $"{cultureName}.json");

            return new[]
            {
                new TranslationFileInfo(new FileInfo(localeFile))
            };
        }

        public virtual void SaveTranslations(TranslationDictionary translationDictionary)
        {
            SaveTranslationsAsync(translationDictionary).Wait();
        }

        public virtual async Task SaveTranslationsAsync(TranslationDictionary translationDictionary)
        {
            var localeFile = GetPathToSave(translationDictionary);

            var translationsToSave = new Dictionary<string, string>(
                translationDictionary.Translations
                                     .Where(_ => !_.MarkedForDelete) // load the entries that are not marked for delete
                                     .OrderBy(_ => _.Key)
                                     .ToDictionary(_ => _.Key.Trim(),
                                                   _ => _.Value.Trim())
                );

            // no file exist, just write everything
            if (!File.Exists(localeFile))
            {
                await File.WriteAllTextAsync(localeFile, JsonConvert.SerializeObject(translationsToSave, Formatting.Indented));
                return;
            }

            var keyToDelete = translationDictionary.Translations
                                                   .Where(_ => _.MarkedForDelete)
                                                   .Select(_ => _.Key);

            var fileContent = await File.ReadAllTextAsync(localeFile);
            var existingTranslation = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);

            // keep the translations that are presented in the existing file but not known by the client side
            foreach (var key in existingTranslation.Keys)
            {
                if (!translationsToSave.ContainsKey(key))
                {
                    translationsToSave[key] = existingTranslation[key];
                }
            }

            foreach (var key in keyToDelete)
            {
                translationsToSave.Remove(key);
            }

            // order translations by key
            translationsToSave = translationsToSave.OrderBy(_ => _.Key)
                                                   .ToDictionary(_ => _.Key.Trim(),
                                                                 _ => _.Value.Trim());

            await File.WriteAllTextAsync(localeFile, JsonConvert.SerializeObject(translationsToSave, Formatting.Indented));
        }

        protected virtual string GetPathToSave(TranslationDictionary translationDictionary)
        {
            return Path.Combine(_webHostEnvironment.ContentRootPath, "Locales", $"{translationDictionary.LocaleName}.json");
        }
    }
}