using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

internal static class ReflectionExtensions
{
    internal static string AssemblySkipLoadingPattern = "^Anonymously|^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|" +
                                                        "^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|" +
                                                        "^DotNetOpenAuth|^EPPlus|^FluentValidation|" +
                                                        "^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|" +
                                                        "^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|" +
                                                        "^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|" +
                                                        "^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|" +
                                                        "^WebActivator|^WebDev|^WebGrease|^NLog|^Handlebars|" +
                                                        "^Npgsql|^Markdig|^LinqToDb|^Twilio";



    public static List<string> GetAvailableFieldNames(this Type    type,
                                                     string       name          = "",
                                                     List<string> recursiveList = null,
                                                     int          level         = 0)
    {
        if (recursiveList == null)
            recursiveList = new List<string>();

        if (level > 3)
            return recursiveList;

        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            recursiveList.Add($"{name}.{property.Name}".Trim('.'));

            if (property.PropertyType.IsClass && property.PropertyType.IsNotSystemType())
            {
                GetAvailableFieldNames(property.PropertyType,
                                      $"{name}.{property.Name}".Trim('.'),
                                      recursiveList,
                                      level + 1);
            }
        }

        return recursiveList;
    }

    public static Type[] GetDerivedTypes<TParent>(this IEnumerable<Assembly> assemblies)
    {
        return GetDerivedTypes(assemblies, typeof(TParent)).ToArray();
    }

    public static Type[] GetDerivedTypes<TParent>(this Assembly assembly)
    {
        return GetDerivedTypes(new[] {assembly}, typeof(TParent)).ToArray();
    }

    public static IEnumerable<Assembly> FilterSkippedAssemblies(this IEnumerable<Assembly> assemblies)
    {
        return assemblies.Where(x => !x.FullName.IsMatch(AssemblySkipLoadingPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }

    public static bool IsNotSystemType(this Type type)
    {
        return type.Namespace == null || (!type.Namespace.IsMatch(AssemblySkipLoadingPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
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
                        !type.IsAbstract && !type.IsInterface)
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

    public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return type != null && type.GetCustomAttribute<TAttribute>() != null;
    }

    public static bool HasProperty<TType>(this Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var property = type.GetProperties()
                           .FirstOrDefault(_ => _.Name == propertyName && _.PropertyType == typeof(TType));

        return property != null;
    }

    public static bool HasAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
    {
        return type != null && type.GetCustomAttribute<TAttribute>() != null;
    }
	
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}