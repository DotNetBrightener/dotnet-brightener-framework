using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Modular.Parsers;

public class JsonModuleDefinitionParser : IModuleDefinitionParser
{
    private readonly ILogger _logger;

    public JsonModuleDefinitionParser(ILogger<JsonModuleDefinitionParser> logger)
    {
        _logger = logger;
    }

    public List<ModuleEntry> LoadAndParseModulesFromFolder(string moduleFolder)
    {
        var moduleEntries = LoadModuleDefinitionsFromFolder(moduleFolder,
                                                            ModuleConstants.ModuleDescriptorFileName);

        return moduleEntries.ToList();
    }

    private IEnumerable<ModuleEntry> LoadModuleDefinitionsFromFolder(string expectingModuleRootFolder,
                                                                     string moduleDefinitionFileName)
    {
        var directoryInfo = new DirectoryInfo(expectingModuleRootFolder);
        var lookingModuleFile = directoryInfo.GetFiles(moduleDefinitionFileName, SearchOption.AllDirectories)
                                              // excludes files that are found in bin and obj folders
                                             .Where(_ => !_.FullName.Contains("\\bin\\") &&
                                                         !_.FullName.Contains("\\obj\\"))
                                             .ToArray();

        if (lookingModuleFile.Length > 0)
        {
            foreach (var file in lookingModuleFile)
            {
                if (file.Exists)
                {
                    var jsonContent      = File.ReadAllText(file.FullName);
                    var moduleDef        = JsonConvert.DeserializeObject<ModuleDefinition>(jsonContent);
                    var moduleFolderPath = Path.GetDirectoryName(file.FullName);

                    var entry = new ModuleEntry(moduleDef)
                    {
                        ModuleFolderPath = moduleFolderPath, 
                        BinPath          = moduleFolderPath
                    };

                    yield return entry;
                }
            }
        }
    }
}