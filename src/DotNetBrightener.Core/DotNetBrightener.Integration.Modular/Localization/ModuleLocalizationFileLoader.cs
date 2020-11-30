using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetBrightener.Core.Localization.Services;
using DotNetBrightener.Core.Modular;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace DotNetBrightener.Integration.Modular.Localization
{
    public class ModuleLocalizationFileLoader : ILocalizationFileLoader
    {
        private readonly LoadedModuleEntries _loadedModules;

        public ModuleLocalizationFileLoader(LoadedModuleEntries loadedModules)
        {
            _loadedModules = loadedModules;
        }

        public IEnumerable<IFileInfo> LoadTranslations(string cultureName)
        {
            var moduleEntries = _loadedModules.Where(_ => _.Name != ModuleEntry.MainModuleIdentifier);

            var translationFiles = moduleEntries
                                   // load locale file path
                                  .Select(moduleEntry =>
                                              Path.Combine(moduleEntry.ModuleFolderPath, "Locales",
                                                           $"{cultureName}.json"))
                                   // return as file object
                                  .Select(localeFilePath => new PhysicalFileInfo(new FileInfo(localeFilePath)))
                                  .Cast<IFileInfo>()
                                  .ToList();

            var mainModule = _loadedModules.FirstOrDefault(_ => _.Name == ModuleEntry.MainModuleIdentifier);

            // main module's locale files can override the other modules' translations, load it at last
            if (mainModule != null)
            {
                var localeFilePath = Path.Combine(mainModule.ModuleFolderPath, "Locales", $"{cultureName}.json");
                translationFiles.Add(new PhysicalFileInfo(new FileInfo(localeFilePath)));
            }

            return translationFiles;
        }
    }
}