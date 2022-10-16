using System.Linq;
using System.Reflection;

namespace System;

internal static class AttributeCheckingUtils
{
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
}