using DotNetBrightener.Core.Modular.Parsers;
using DotNetBrightener.Core.Modular.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace DotNetBrightener.Core.Modular.Extensions;

public static class ModularEnabledServiceCollectionExtensions
{
    public static IServiceCollection AddModular(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IModuleDefinitionParser, JsonModuleDefinitionParser>();
        serviceCollection.AddSingleton<ISpaModuleHandler, DefaultSpaModuleHandler>();
        serviceCollection.AddSingleton<IModuleLoader, ModuleLoader>();
        serviceCollection.AddSingleton<IModuleAssembliesLoader, ModuleAssembliesLoader>();
            
        IModuleAssembliesLoader moduleAssembliesLoader;

        using (var serviceProvider = serviceCollection.BuildServiceProvider())
        {
            moduleAssembliesLoader = serviceProvider.GetService<IModuleAssembliesLoader>();

            var moduleLoader = serviceProvider.GetService<IModuleLoader>();
            var allModules = moduleLoader.LoadAvailableModules()
                                         .Cast<ModuleEntry>()
                                         .ToArray();

            foreach (var module in allModules)
            {
                moduleAssembliesLoader.LoadModuleAssemblies(module);

                module.ModuleTypeMetadataCollection.ForEach(typeMetadata =>
                {
                    serviceCollection
                       .AddSingleton(typeof(ITypeMetadata),
                                     typeMetadata);
                });
            }
        }

        AppDomain.CurrentDomain.AssemblyResolve += moduleAssembliesLoader.ResolveAssembly;

        return serviceCollection;
    }

    /// <summary>
    ///     Enables the specified modules and put to the <see cref="IServiceCollection"/> as <see cref="LoadedModuleEntries"/> instance for reference later
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="enabledModules">A collection of the module Ids to enable</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining operations</returns>
    public static LoadedModuleEntries EnableModules(this IServiceCollection serviceCollection,
                                                    string[]                enabledModules = null)
    {
        // ensure only one LoadedModuleEntries available in the given service collection
        var existingLoadedModuleEntries = serviceCollection.Any(_ => _.ServiceType == typeof(LoadedModuleEntries));

        if (existingLoadedModuleEntries)
        {
            serviceCollection.RemoveAll<LoadedModuleEntries>();
        }

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var       moduleLoader    = serviceProvider.GetService<IModuleLoader>();
        var allModules = moduleLoader.RetrieveEnableModules(enabledModules)
                                     .ToArray();

        var loadedModules = new LoadedModuleEntries(allModules);
        serviceCollection.AddSingleton(loadedModules);

        return loadedModules;
    }
}