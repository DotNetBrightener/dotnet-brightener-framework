using System.Collections.Generic;
using System.Linq;
using DotNetBrightener.Core.Modular.Services;

namespace DotNetBrightener.Core.Modular;

public static class ModuleLoaderExtensions
{
    /// <summary>
    ///     Retrieves a collection of enabled <see cref="ModuleEntry"/> records with the given module ids
    /// </summary>
    /// <remarks>
    ///     It will load all system modules by default, if the <see cref="enableModules"/> is not defined or equals "*" all extension modules wil also be loaded
    /// </remarks>
    /// <param name="moduleLoader"></param>
    /// <param name="enabledModuleIds">Collection of module names which are enabled</param>
    /// <returns>
    ///     A collection of enabled <see cref="ModuleEntry"/> records
    /// </returns>
    public static IEnumerable<ModuleEntry> RetrieveEnableModules(this IModuleLoader moduleLoader,
                                                                 string[]           enabledModuleIds = null)
    {
        var allAvailableModules = moduleLoader.LoadAvailableModules()
                                              .OfType<ModuleEntry>()
                                              .ToList();

        enabledModuleIds ??= new[] {"*"};

        var hasLoadAllOption = enabledModuleIds.Any(_ => _.Equals("*"));

        var systemModules =
            allAvailableModules.Where(_ => _.ModuleType == ModuleType.Infrastructure ||
                                           _.ModuleType == ModuleType.SystemModule);

        IEnumerable<ModuleEntry> extensionModules;
        if (hasLoadAllOption)
        {
            // if has load all, we load the extension modules that are not listed as ignored
            extensionModules = allAvailableModules.Where(_ => _.ModuleType == ModuleType.ExtensionModule &&
                                                              !enabledModuleIds.Contains($"-{_.ModuleId}") &&
                                                              !enabledModuleIds.Contains($"--{_.ModuleId}") &&
                                                              !enabledModuleIds.Contains($"[{_.ModuleId}]"));
        }
        else
        {
            // if not load all, we load only modules defined in the EnabledModules
            extensionModules = allAvailableModules.Where(_ => _.ModuleType == ModuleType.ExtensionModule &&
                                                              enabledModuleIds.Contains(_.ModuleId) &&
                                                              !enabledModuleIds.Contains($"-{_.ModuleId}") &&
                                                              !enabledModuleIds.Contains($"--{_.ModuleId}") &&
                                                              !enabledModuleIds.Contains($"[{_.ModuleId}]"));
        }

        var finalEnabledModules = systemModules.Concat(extensionModules);

        return finalEnabledModules;
    }
}