using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetBrightener.Core.Modular.Parsers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Modular.Services;

public class ModuleLoader : IModuleLoader
{
    private readonly IConfiguration                       _configuration;
    private readonly IWebHostEnvironment                  _hostingEnvironment;
    private readonly IEnumerable<IModuleDefinitionParser> _moduleParsers;
    private readonly ILogger                              _logger;
    private static   List<ModuleDefinition>               _moduleDefinitions   = new List<ModuleDefinition>();
    private static   bool                                 _isInitialized       = false;
    private const    string                               UpperFolderIndicator = @"..\";

    public ModuleLoader(IConfiguration                       configuration,
                        IWebHostEnvironment                  hostingEnvironment,
                        IEnumerable<IModuleDefinitionParser> moduleParsers,
                        ILogger<ModuleLoader>                logger)
    {
        _configuration      = configuration;
        _hostingEnvironment = hostingEnvironment;
        _moduleParsers      = moduleParsers;
        _logger             = logger;
    }

    public List<ModuleDefinition> LoadAvailableModules()
    {
        if (_isInitialized && _moduleDefinitions.Any())
        {
            return _moduleDefinitions;
        }

        var basePath = _hostingEnvironment.ContentRootPath;

        LoadModulesFromContainer(basePath,
                                 new[]
                                 {
                                     "Modules",
                                     @$"{UpperFolderIndicator}Modules"
                                 },
                                 _moduleDefinitions);

        var frameworkModules = _moduleDefinitions.Where(_ => _.ModuleType == ModuleType.Infrastructure)
                                                 .Select(_ => _.ModuleId)
                                                 .ToArray();

        // make all modules depends on the framework modules
        _moduleDefinitions.ForEach(module =>
        {
            if (module.ModuleType == ModuleType.Infrastructure)
                return;

            module.Dependencies.InsertRange(0, frameworkModules);
        });

        // then sort them by dependencies.
        // this will keep the modules marked with infrastructure to be on the top of the modules list.
        _moduleDefinitions = _moduleDefinitions.SortByDependencies(definition => _moduleDefinitions
                                                                      .Where(module => definition
                                                                                      .Dependencies
                                                                                      .Contains(module
                                                                                                   .ModuleId)))
                                               .Distinct()
                                               .ToList();

        // put the main module on top of the list
        _moduleDefinitions.Insert(0, new ModuleEntry
        {
            BinPath          = _hostingEnvironment.ContentRootPath,
            ModuleFolderPath = Path.GetDirectoryName(_hostingEnvironment.ContentRootPath),
            Name             = ModuleEntry.MainModuleIdentifier,
            ModuleType       = ModuleType.Infrastructure,
            ModuleId         = ModuleEntry.MainModuleIdentifier
        });

        LoadModuleConfigurations();

        var serializedModuleEntries = JsonConvert.SerializeObject(_moduleDefinitions, Formatting.Indented);
        _logger.LogInformation($"Loaded modules: {serializedModuleEntries}");
        _isInitialized = true;
        return _moduleDefinitions;
    }

    private void LoadModulesFromContainer(string                 basePath,
                                          string[]               modulePaths,
                                          List<ModuleDefinition> modulesList)
    {
        foreach (var path in modulePaths)
        {
            var parentFolder     = new DirectoryInfo(basePath);
            var actualFolderPath = path;

            if (path.StartsWith(UpperFolderIndicator))
            {
                var numberOfParentLevels = path.Split(new[] {UpperFolderIndicator}, StringSplitOptions.None)
                                               .Count(x => x == string.Empty);
                while (numberOfParentLevels > 0)
                {
                    if (parentFolder?.Parent != null)
                    {
                        parentFolder = parentFolder.Parent;
                    }

                    numberOfParentLevels--;
                }

                actualFolderPath = path.Split(new[] {UpperFolderIndicator}, StringSplitOptions.RemoveEmptyEntries)
                                       .FirstOrDefault();
            }

            var moduleContainerFolder = Path.Combine(parentFolder.FullName, actualFolderPath);

            if (!Directory.Exists(moduleContainerFolder))
                continue;

            var moduleFolders = Directory.GetDirectories(moduleContainerFolder);

            foreach (var folder in moduleFolders)
            {
                var moduleEntries = LoadModuleInfoFromFolder(folder);
                if (moduleEntries == null || !moduleEntries.Any())
                    continue;

                modulesList.AddRange(moduleEntries);
            }
        }
    }

    private List<ModuleEntry> LoadModuleInfoFromFolder(string folder)
    {
        var moduleDefinitions = _moduleParsers
                               .SelectMany(moduleParser => moduleParser.LoadAndParseModulesFromFolder(folder))
                               .ToList();

        return moduleDefinitions;
    }

    /// <summary>
    /// Loads the configurations for the modules from their appsettings.json file
    /// </summary>
    private void LoadModuleConfigurations()
    {
        foreach (var moduleDefinition in _moduleDefinitions.Cast<ModuleEntry>())
        {
            LoadModuleConfig(moduleDefinition);
            moduleDefinition.DependencyModules = _moduleDefinitions
                                                .Where(_ => moduleDefinition.Dependencies.Contains(_.ModuleId))
                                                .Cast<ModuleEntry>()
                                                .ToList();
        }
    }


    /// <summary>
    /// Loads the configuration for the specified module from its appsettings.json file
    /// </summary>
    private void LoadModuleConfig(ModuleEntry moduleEntry)
    {
        var p = moduleEntry.BinPath;
        if (!p.EndsWith("bin"))
            p = Path.Combine(p, "bin");

        var binDirectory = Directory.Exists(p) ? new DirectoryInfo(p) : new DirectoryInfo(moduleEntry.BinPath);

        moduleEntry.BinPath = binDirectory.FullName;

        var configurationFolder = moduleEntry.BinPath;

        const string appSettingsFileName = "appsettings.json";
        var          maxFind             = 3;

        while (maxFind > 0)
        {
            if (!Directory.EnumerateFiles(configurationFolder,
                                          appSettingsFileName,
                                          SearchOption.TopDirectoryOnly)
                          .Any())
                configurationFolder = Path.GetDirectoryName(configurationFolder);
            else
            {
                break;
            }

            maxFind--;
        }

        if (maxFind <= 0)
            return;

        var builder = new ConfigurationBuilder()
                     .SetBasePath(configurationFolder)
                     .AddJsonFile(appSettingsFileName, optional: true, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{_hostingEnvironment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables()
                     .AddConfiguration(_configuration);

        moduleEntry.Configuration = builder.Build();
    }
}