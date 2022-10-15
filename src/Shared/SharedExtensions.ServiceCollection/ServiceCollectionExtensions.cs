using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

internal static class ServiceCollectionExtensions
{
    public static Type[] GetDerivedTypes<TParent>(this IEnumerable<Assembly> assemblies)
    {
        return GetDerivedTypes(assemblies, typeof(TParent)).ToArray();
    }

    public static Type[] GetDerivedTypes<TParent>(this Assembly assembly)
    {
        return GetDerivedTypes(new[]
                               {
                                   assembly
                               },
                               typeof(TParent))
           .ToArray();
    }

    private static List<Type> GetDerivedTypes(this IEnumerable<Assembly> bundledAssemblies, Type t)
    {
        var result = new List<Type>();

        foreach (var assembly in bundledAssemblies)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }

            foreach (Type type in types)
            {
                try
                {
                    if ((t.IsAssignableFrom(type) ||
                         type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == t)) &&
                        !type.IsAbstract &&
                        !type.IsInterface)
                    {
                        result.Add(type);
                    }
                }
                catch (Exception err)
                {
                }
            }
        }

        return result;
    }
}