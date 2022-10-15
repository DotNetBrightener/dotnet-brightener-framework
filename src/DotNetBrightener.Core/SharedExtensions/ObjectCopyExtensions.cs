using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System;

internal static class ObjectCopyExtensions
{
    public static bool CopyTo<TIn, TOut>(this TIn                      entityIn,
                                         TOut                          entityOut,
                                         bool                          copyNullValues    = false,
                                         Expression<Func<TIn, object>> ignoredProperties = null)
    {
        var ignoredPropertiesExpression = ignoredProperties != null
                                              ? ignoredProperties.GetSimplePropertyAccessList()
                                              : Enumerable.Empty<PropertyInfo>();


        var sourceProperties = entityIn.GetType().GetProperties();

        var filteredProperties =
            sourceProperties.Where(x => !ignoredPropertiesExpression.Select(y => y.Name).Contains(x.Name));

        var changed = false;
        foreach (var prop in filteredProperties)
        {
            var destProp = entityOut.GetType().GetProperty(prop.Name);

            if (destProp == null || prop.PropertyType != destProp.PropertyType || !destProp.CanWrite)
                continue;

            if (!prop.CanRead)
                continue;

            var sourceValue = prop.GetValue(entityIn);
            var destValue   = destProp.GetValue(entityOut);


            if (sourceValue == null && (!copyNullValues || destValue == null))
                continue;

            if (sourceValue != null && sourceValue.Equals(destValue))
                continue;

            destProp.SetValue(entityOut, sourceValue, null);
            changed = true;
        }

        return changed;
    }

    public static object TryGetValue<T>(T model, string fromField, Type propertyType = null)
    {
        var properties = fromField.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();

        var propertiesStack = new Stack<string>(properties);

        PropertyInfo property = null;

        var propertyName = propertiesStack.Pop();

        property = propertyType != null
                       ? propertyType.GetProperty(propertyName)
                       : typeof(T).GetProperty(propertyName);

        if (property == null)
        {
            if (typeof(DynamicObject).IsAssignableFrom(propertyType))
            {
                try
                {

                    return ((dynamic) model)[propertyName];
                }
                catch (Exception exception)
                {
                }
            }

            return null;
        }

        return property.GetValue(model);
    }

    public static void TrySetValue<T>(T model, string toField, object value, Type propertyType = null)
    {
        var properties = toField.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();

        var propertiesStack = new Stack<string>(properties);

        PropertyInfo property = null;

        var propertyName = propertiesStack.Pop();

        property = propertyType != null
                       ? propertyType.GetProperty(propertyName)
                       : typeof(T).GetProperty(propertyName);

        if (property == null)
        {
            if (typeof(DynamicObject).IsAssignableFrom(propertyType))
            {
                try
                {

                    ((dynamic) model)[propertyName] = value;
                }
                catch (Exception exception)
                {
                }
            }

            return;
        }


        var propValue = property.GetValue(model);

        var propertyT = propValue?.GetType() ?? property.PropertyType;

        if (!propertyT.IsPrimitive && propertyT.Assembly.GetName().Name != "mscorlib")
        {
            if (propValue == null)
            {
                try
                {
                    property.SetValue(model, Activator.CreateInstance(propertyT));
                    propValue = property.GetValue(model);
                }
                catch (Exception exception)
                {

                }
            }

            if (propValue == null)
                return;

            TrySetValue(propValue, string.Join(".", propertiesStack.ToArray()), value, propertyT);
            property.SetValue(model, propValue);
        }
        else if (property.CanWrite)
        {
            var convertType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (value == null) return;

            try
            {
                var convertedValue = Convert.ChangeType(value, convertType);

                property.SetValue(model, convertedValue, null);
            }
            catch (Exception exception)
            {
                if (convertType == typeof(DateTime) && value is DateTimeOffset)
                {
                    var dateTimeValue = ((DateTimeOffset) value).DateTime;
                    property.SetValue(model, dateTimeValue, null);
                }

            }
        }
    }
}