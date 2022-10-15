using DotNetBrightener.Core.Localization;
using DotNetBrightener.Core.Localization.Services;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetBrightener.Core.Modular.Localization;

/// <summary>
///     Represents the <see cref="ILocalizationFileManager" /> for modular architecture
/// </summary>
public class ModuleLocalizationFileManager : DefaultLocalizationFileManager
{
    private readonly LoadedModuleEntries _loadedModules;

    public ModuleLocalizationFileManager(IWebHostEnvironment webHostEnvironment,
                                         LoadedModuleEntries loadedModules)
        : base(webHostEnvironment)
    {
        _loadedModules = loadedModules;
    }

    public override IEnumerable<ITranslationFileInfo> LoadTranslationFiles(string cultureName)
    {
        var moduleEntries = _loadedModules.Where(_ => _.Name != ModuleEntry.MainModuleIdentifier);

        var translationFiles = moduleEntries
                               // load locale file path
                              .Select(moduleEntry =>
                               {
                                   var localeFilePath = Path.Combine(moduleEntry.ModuleFolderPath, "Locales", $"{cultureName}.json");

                                   return new TranslationFileInfo(new FileInfo(localeFilePath))
                                   {
                                       ModuleId   = moduleEntry.ModuleId,
                                       ModuleName = moduleEntry.Name,
                                       LocaleName = cultureName
                                   };
                               })
                              .ToList();

        var mainModule = _loadedModules.FirstOrDefault(_ => _.Name == ModuleEntry.MainModuleIdentifier);

        // main module's locale files can override the other modules' translations, load it at last
        if (mainModule != null)
        {
            var localeFilePath = Path.Combine(mainModule.ModuleFolderPath, "Locales", $"{cultureName}.json");
            translationFiles.Add(new TranslationFileInfo(new FileInfo(localeFilePath))
            {
                LocaleName = cultureName,
                ModuleId   = mainModule.ModuleId,
                ModuleName = mainModule.Name
            });
        }

        return translationFiles;
    }

    protected override string GetPathToSave(TranslationDictionary translationDictionary)
    {
        if (!string.IsNullOrEmpty(translationDictionary.ModuleId))
        {
            var moduleEntry = _loadedModules.FirstOrDefault(_ => _.ModuleId == translationDictionary.ModuleId);
            if (moduleEntry != null)
            {
                return Path.Combine(moduleEntry.ModuleFolderPath, "Locales", $"{translationDictionary.LocaleName}.json");
            }
        }

        return base.GetPathToSave(translationDictionary);
    }
}