using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace System;

internal static class ObjectExtensions
{
    public static void TryAssignValue<T>(this T obj, string propertyName, object value)
    {
        var property = typeof(T).GetProperties()
                                .FirstOrDefault(_ => _.Name == propertyName);

        if (property == null || !property.CanWrite)
            return;

        try
        {
            property.SetValue(obj, value);
        }
        catch (Exception exception)
        {

        }
    }

    /// <summary>
    /// Used to simplify and beautify casting an object to a type. 
    /// </summary>
    /// <typeparam name="T">Type to be casted</typeparam>
    /// <param name="obj">Object to cast</param>
    /// <returns>Casted object</returns>
    public static T As<T>(this object obj)
        where T : class
    {
        return (T) obj;
    }

    /// <summary>
    /// Converts given object to a value type using <see cref="Convert.ChangeType(object,System.TypeCode)"/> method.
    /// </summary>
    /// <param name="obj">Object to be converted</param>
    /// <typeparam name="T">Type of the target object</typeparam>
    /// <returns>Converted object</returns>
    public static T To<T>(this object obj)
        where T : struct
    {
        if (typeof(T) == typeof(Guid))
        {
            return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(obj.ToString());
        }

        return (T) Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Check if an item is in a list.
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <param name="list">List of items</param>
    /// <typeparam name="T">Type of the items</typeparam>
    public static bool IsIn<T>(this T item, params T[] list)
    {
        return list.Contains(item);
    }

    public static T? ToNullable<T>(this object obj) where T : struct
    {
        if (obj == null)
        {
            return default(T);
        }

        var objString = obj as string;
        if (objString != null)
        {
            return objString.ToNullable<T>();
        }

        var objType = typeof(T);
        if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            objType = Nullable.GetUnderlyingType(objType);
        }

        return (T) Convert.ChangeType(obj, objType);
    }

    public static T Pop<T>(this List<T> list) where T : class
    {
        if (!list.Any())
            return null;

        var element = list.Last();
        list.RemoveAt(list.Count - 1);
        return element;
    }
}