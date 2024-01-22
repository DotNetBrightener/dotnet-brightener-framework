using System;
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
    public static EntityMetadata<TType> ExtractMetadata<TType>()
    {
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

        var exampleJson = Activator.CreateInstance<TType>();

        return new EntityMetadata<TType>
        {
            Columns = columns.ToArray()
        };
    }

    internal static string[] GetDefaultColumns<TType>()
    {
        return typeof(TType).GetProperties()
                            .Where(_ => !_.HasAttribute<JsonIgnoreAttribute>() &&
                                        !_.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>())
                            .Select(_ => _.Name)
                            .ToArray();
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

public class EntityMetadata<TType>
{
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public EntityColumn[] Columns { get; set; }

    public string Description => typeof(TType).GetXmlDocumentation();

    public string Name => typeof(TType).Name;

    public Dictionary<string, object> ExampleJson
    {
        get
        {
            var exampleJson = Activator.CreateInstance<TType>();

            var serializeObject = JsonConvert.SerializeObject(exampleJson,
                                                              _jsonSerializerSettings);

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