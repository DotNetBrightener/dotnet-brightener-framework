using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.Modular;

public static class ModuleEntriesListExtensions
{
    /// <summary>
    ///     Retrieves the <see cref="ModuleEntry"/> that contains the specified <see cref="assembly"/>
    /// </summary>
    /// <param name="moduleEntries">
    ///     
    /// </param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static ModuleEntry GetAssociatedModuleEntry(this List<ModuleEntry> moduleEntries, Assembly assembly)
    {
        var moduleEntry = moduleEntries.FirstOrDefault(_ => _.ModuleAssemblies.Contains(assembly));

        return moduleEntry;
    }

    /// <summary>
    ///     Retrieves the <see cref="ModuleEntry"/> that contains the type of the specified instance
    /// </summary>
    /// <param name="moduleEntries"></param>
    /// <param name="classInstance"></param>
    /// <returns></returns>
    public static ModuleEntry GetAssociatedModuleEntry(this List<ModuleEntry> moduleEntries, Type classInstance)
    {
        var classAssembly = classInstance.Assembly;

        var moduleEntry = moduleEntries.FirstOrDefault(_ => _.ModuleAssemblies.Contains(classAssembly));

        return moduleEntry;
    }

    /// <summary>
    /// Retrieves the <see cref="ModuleEntry"/> that contains the type of the specified instance
    /// </summary>
    /// <param name="moduleEntries"></param>
    /// <param name="classInstance"></param>
    /// <returns></returns>
    public static ModuleEntry GetAssociatedModuleEntry(this List<ModuleEntry> moduleEntries, object classInstance)
    {
        var classAssembly = classInstance.GetType().Assembly;

        var moduleEntry = moduleEntries.FirstOrDefault(_ => _.ModuleAssemblies.Contains(classAssembly));

        return moduleEntry;
    }

    /// <summary>
    ///     Sorts the collection of <typeparamref name="T"/> objects by the order of associated modules which are loaded to the system
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection</typeparam>
    /// <param name="sourceEnumerable">The collection of <typeparamref name="T"/> to sort</param>
    /// <param name="orderedModules">The loaded modules, which already sorted in dependency order</param>
    /// <param name="descendingOrder">Indicates whether the sorted collection is in descending order</param>
    /// <returns>
    ///     Ordered collection of <typeparamref name="T"/> objects in the module dependencies order.
    /// </returns>
    public static IEnumerable<T> OrderByModuleDependencies<T>(this IEnumerable<T> sourceEnumerable,
                                                              List<ModuleEntry>   orderedModules,
                                                              bool                descendingOrder = false)
    {
        int OrderExpression(T _) => orderedModules.IndexOf(orderedModules.GetAssociatedModuleEntry(_));

        return descendingOrder
                   ? sourceEnumerable.OrderByDescending(OrderExpression)
                   : sourceEnumerable.OrderBy(OrderExpression);
    }
}