using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static class EntityMetadataExtractor
{
    private static readonly ConcurrentDictionary<Type, string[]>       DefaultColumnsMappings = new();
    private static readonly ConcurrentDictionary<Type, string[]>       IgnoreColumnsMappings  = new();
    private static readonly ConcurrentDictionary<Type, EntityMetadata> EntityMetadataMappings = new();

    public static string[] GetIgnoredProperties(this Type type)
    {
        if (IgnoreColumnsMappings.TryGetValue(type, out var mapping))
            return mapping;

        var mappings = type.GetProperties()
                           .Where(prop => prop.HasAttribute<JsonIgnoreAttribute>() ||
                                          prop.HasAttribute<
                                              System.Text.Json.Serialization.JsonIgnoreAttribute>())
                           .Select(prop => prop.Name)
                           .ToArray();

        IgnoreColumnsMappings.TryAdd(type, mappings);

        return mappings;
    }

    public static EntityMetadata<TType> ExtractMetadata<TType>()
    {
        if (EntityMetadataMappings.TryGetValue(typeof(TType), out var metadata) && 
            metadata is EntityMetadata<TType> metadataValue)
            return metadataValue;

        var properties = typeof(TType).GetProperties();

        var columns = new List<EntityColumn>();

        foreach (var property in properties)
        {
            if (property.HasAttribute<JsonIgnoreAttribute>() ||
                property.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>())
                continue;

            var propertyTypeName = property.PropertyType.GetGenericArguments().FirstOrDefault()?.Name ??
                                   property.PropertyType.Name;
            var column = new EntityColumn
            {
                Name        = property.Name.ToCamelCase(),
                Description = property.GetXmlDocumentation(),
                Type        = propertyTypeName,
                IsNullable  = property.PropertyType.IsNullable(),
                IsRequired  = property.HasAttribute<RequiredAttribute>(),
                MaxLength   = property.GetMaxLength(),
                MinLength   = property.GetMinLength()
            };

            columns.Add(column);
        }

        metadataValue = new EntityMetadata<TType>
        {
            Columns = columns.ToArray()
        };

        EntityMetadataMappings.TryAdd(typeof(TType), metadataValue);

        return metadataValue;
    }

    internal static string[] GetDefaultColumns(this Type inputType, string prefix = "", int level = 1)
    {
        if (DefaultColumnsMappings.TryGetValue(inputType, out var defaultColumns))
            return defaultColumns;

        defaultColumns = inputType.GetProperties()
                                  .SelectMany(property =>
                                   {
                                       var propName      = prefix + property.Name;
                                       var innerPropType = property.PropertyType;

                                       if (innerPropType.IsClass &&
                                           innerPropType != typeof(string) &&
                                           level <= 2)
                                       {
                                           // Recursive call for nested classes
                                           return GetDefaultColumns(innerPropType, propName + ".", level + 1);
                                       }

                                       if (property.HasAttribute<JsonIgnoreAttribute>() ||
                                           property.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>())
                                       {
                                           return Array.Empty<string>();
                                       }

                                       return new[]
                                       {
                                           propName
                                       };
                                   })
                                  .Concat(new[]
                                   {
                                       prefix.Trim('.')
                                   })
                                  .Where(s => !string.IsNullOrEmpty(s))
                                  .Distinct()
                                  .ToArray();

        DefaultColumnsMappings.TryAdd(inputType, defaultColumns);

        return defaultColumns;
    }

    internal static int? GetMaxLength(this PropertyInfo property)
    {
        var maxLengthAttribute = property.GetCustomAttribute<MaxLengthAttribute>();

        return maxLengthAttribute?.Length;
    }

    internal static int? GetMinLength(this PropertyInfo property)
    {
        var minLengthAttribute = property.GetCustomAttribute<MinLengthAttribute>();

        return minLengthAttribute?.Length;
    }


    /// <summary>
    ///     Convert a given string to camelCaseString
    /// </summary>
    /// <param name="str">
    ///     Input string
    /// </param>
    /// <returns>
    ///     The string returned in camelCase format
    /// </returns>
    internal static string ToCamelCase(this string str)
    {
        var words = str.Split(new[]
                              {
                                  "_", " "
                              },
                              StringSplitOptions.RemoveEmptyEntries);
        var leadWord = Regex.Replace(words[0],
                                     @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
                                     m => m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() +
                                          m.Groups[3].Value);

        var tailWords = words.Skip(1)
                             .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                             .ToArray();

        return $"{leadWord}{string.Join(string.Empty, tailWords)}";
    }
}

public class EntityMetadata
{
    protected static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
}

public class EntityMetadata<TType> : EntityMetadata
{

    public EntityColumn[] Columns { get; set; }

    public string Description => typeof(TType).GetXmlDocumentation();

    public string Name => typeof(TType).Name;

    public Dictionary<string, object> ExampleJson
    {
        get
        {
            var exampleJson = Activator.CreateInstance<TType>();

            var serializeObject = JsonConvert.SerializeObject(exampleJson,
                                                              JsonSerializerSettings);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(serializeObject);
        }
    }
}


public class EntityColumn
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }

    public bool IsNullable { get; set; }

    public bool IsRequired { get; set; }

    public int? MaxLength { get; set; }

    public int? MinLength { get; set; }
}