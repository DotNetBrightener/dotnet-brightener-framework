using System;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.DataAccess.Attributes;

/// <summary>
///     Marks the associated property to not be updated by the Data transfer object.
///     The value for the property came from Data Transfer Object will not be copied over to the entity
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NoClientSideUpdateAttribute: Attribute
{
        
}

public static class NoClientSideUpdateHelper
{
    public static string [ ] GetPropertiesWithNoClientSideUpdate(this Type entityType)
    {
        return entityType.GetProperties()
                         .Where(prop => prop.HasAttribute<NoClientSideUpdateAttribute>())
                         .Select(prop => prop.Name)
                         .ToArray();
    }
}