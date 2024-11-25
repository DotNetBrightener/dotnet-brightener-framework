using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DotNetBrightener.Core.Logging.Internals;

internal static class DataTransferObjectUtils
{
    /// <summary>
    ///     Updates the given <see cref="entityObject"/> by the values provided in the <seealso cref="dataTransferObject"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="entityObject"/>
    /// </typeparam>
    /// <param name="entityObject">
    ///     The object to be updated
    /// </param>
    /// <param name="dataTransferObject">
    ///     The object contains the data to apply updates to <see cref="entityObject"/>
    /// </param>
    /// <param name="ignoreProperties">
    ///     The properties that should not be updated by this method
    /// </param>
    /// <returns>
    ///     The <see cref="entityObject"/> itself
    /// </returns>
    public static T UpdateFromDto<T>(this T               entityObject,
                                     object               dataTransferObject,
                                     params string[]      ignoreProperties) where T : class
    {
        Type entityType = typeof(T);

        var jobject           = JObject.FromObject(dataTransferObject);
        var propertiesFromDto = jobject.Properties();

        foreach (var propertyInfo in propertiesFromDto)
        {
            if (ignoreProperties.Contains(propertyInfo.Name))
                continue;

            var csConventionName = propertyInfo.Name[0].ToString().ToUpper() + propertyInfo.Name.Substring(1);

            if (ignoreProperties.Contains(csConventionName))
                continue;

            if (!TryPickPropAndValue(entityType, propertyInfo, out var destinationProp, out var value))
                continue;

            var oldValue = destinationProp.GetValue(entityObject);

            if (oldValue?.Equals(value) == true)
                continue;
            
            destinationProp.SetValue(entityObject, value);
        }

        return entityObject;
    }

    private static bool TryPickPropAndValue(Type             entityType,
                                            JProperty        propertyFromDto,
                                            out PropertyInfo propertyOnEntity,
                                            out object       valueToUpdate)
    {
        valueToUpdate    = null;
        propertyOnEntity = GetProperty(entityType, propertyFromDto.Name);

        // not converting some properties that should not be put back to the entity
        if (propertyOnEntity == null ||
            propertyOnEntity.HasAttribute<NotMappedAttribute>())
            return false;

        if (propertyOnEntity.HasAttribute<KeyAttribute>())
        {
            var keyPropAttr = propertyOnEntity.GetCustomAttribute<DatabaseGeneratedAttribute>();

            if (keyPropAttr == null ||
                keyPropAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                return false;
        }

        if (!propertyOnEntity.CanWrite)
            return false;

        if (propertyOnEntity.GetGetMethod()?.IsVirtual == true)
        {
            return false;
        }

        valueToUpdate = propertyFromDto.Value.ToObject(propertyOnEntity.PropertyType);

        if (valueToUpdate != null)
        {
            if (propertyOnEntity.PropertyType == typeof(DateTime) &&
                valueToUpdate is DateTime dateTimeValue &&
                dateTimeValue == DateTime.MinValue)
            {
                valueToUpdate = new DateTime(1970, 1, 1);
            }

            else if (propertyOnEntity.PropertyType == typeof(DateTimeOffset) &&
                     valueToUpdate is DateTimeOffset dateTimeOffsetValue &&
                     dateTimeOffsetValue == DateTimeOffset.MinValue)
            {
                valueToUpdate = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);
            }
        }

        return true;
    }

    internal static PropertyInfo GetProperty<T>(string propName) where T : class
    {
        return GetProperty(typeof(T), propName);
    }

    internal static PropertyInfo GetProperty(Type type, string propName)
    {
        PropertyInfo prop = type.GetProperty(propName);

        if (prop == null)
        {
            var csConventionName = propName[0].ToString().ToUpper() + propName.Substring(1);
            prop = type.GetProperty(csConventionName);
        }

        return prop;
    }


    internal static bool HasAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
    {
        return type?.GetCustomAttribute<TAttribute>() != null;
    }
}