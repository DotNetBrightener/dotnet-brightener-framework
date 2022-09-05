using System;
using System.Collections.Generic;

namespace DotNetBrightener.PublisherTool.CLI;

internal static class ModuleSorterExtension
{
    /// <summary>
    ///     Orders the input <typeparamref name="T"/> based on its element's dependencies
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IEnumerable{T}"/></typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="dependencies">The expression returns the dependencies collection</param>
    /// <param name="throwOnCircularDepsCaught">Indicates whether should throw exception when circular dependencies are found</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> of the sorted source by its elements' dependencies</returns>
    public static IEnumerable<T> SortByDependencies<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCircularDepsCaught = false)
    {
        var sorted  = new List<T>();
        var visited = new HashSet<T>();

        foreach (var item in source)
            Visit(item, visited, sorted, dependencies, throwOnCircularDepsCaught);

        return sorted;
    }

    private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
    {
        if (!visited.Contains(item))
        {
            visited.Add(item);

            foreach (var dep in dependencies(item))
                Visit(dep, visited, sorted, dependencies, throwOnCycle);

            sorted.Add(item);
        }
        else
        {
            if (throwOnCycle && !sorted.Contains(item))
                throw new Exception("Circular dependency found");
        }
    }
}