using System;
using System.Reflection;

namespace DotNetBrightener.Core.Modular.Services;

public interface IModuleAssembliesLoader
{
    /// <summary>
    /// Loads the assemblies for specified module
    /// </summary>
    /// <param name="moduleEntry"></param>
    /// <returns></returns>
    Assembly[] LoadModuleAssemblies(ModuleEntry moduleEntry);

    AssemblyName[] LoadAssemblyNames();

    Assembly ResolveAssembly(object sender, ResolveEventArgs resolveArgs);
}