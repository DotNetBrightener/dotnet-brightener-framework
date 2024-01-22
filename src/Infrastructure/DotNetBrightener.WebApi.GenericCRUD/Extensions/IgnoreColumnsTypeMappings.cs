using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

internal static class IgnoreColumnsTypeMappings
{
    public static ConcurrentDictionary<Type, string[]> IgnoreColumnsMappings = new();

    public static string[] RetrieveIgnoreColumns<TType>()
    {
        if (IgnoreColumnsMappings.TryGetValue(typeof(TType), out var mapping))
            return mapping;

        var mappings = typeof(TType).GetProperties()
                                    .Where(prop => prop.HasAttribute<JsonIgnoreAttribute>() ||
                                                   prop.HasAttribute<
                                                       System.Text.Json.Serialization.JsonIgnoreAttribute>())
                                    .Select(_ => _.Name)
                                    .ToArray();

        IgnoreColumnsMappings.TryAdd(typeof(TType), mappings);

        return mappings.ToArray();
    }
}