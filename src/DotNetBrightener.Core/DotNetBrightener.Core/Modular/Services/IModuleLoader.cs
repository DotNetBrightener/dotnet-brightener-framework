using System.Collections.Generic;

namespace DotNetBrightener.Core.Modular.Services;

public interface IModuleLoader
{
    /// <summary>
    /// Loads all available modules in the system
    /// </summary>
    /// <returns></returns>
    List<ModuleDefinition> LoadAvailableModules();
}