using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System;

internal static class ObjectMethodExtensions
{
    /// <summary>
    ///     Retrieve the <see cref="MethodInfo"/> with the specified name from the given type. Private method will be included in the search
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static MethodInfo GetMethodWithName(this Type type, string methodName)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .FirstOrDefault(_ => _.Name.Equals(methodName));
    }

    /// <summary>
    ///     Retrieve the <see cref="MethodInfo"/> with the specified name from the given instance of object. Private method will be included in the search
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static MethodInfo GetMethodWithName<T>(this T obj, string methodName)
    {
        return obj.GetType().GetMethodWithName(methodName);
    }

}