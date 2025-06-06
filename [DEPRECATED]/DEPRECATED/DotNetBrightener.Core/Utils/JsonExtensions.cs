﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.Core.Utils;

public static class JsonExtensions
{
    /// <summary>
    /// Converts given object to JSON string.
    /// </summary>
    /// <returns></returns>
    public static string ToJsonString(this object obj, bool camelCase = false, bool indented = false)
    {
        var options = new JsonSerializerSettings
        {
            ContractResolver = camelCase
                                   ? new CamelCasePropertyNamesContractResolver()
                                   : new DefaultContractResolver()
        };


        if (indented)
        {
            options.Formatting = Formatting.Indented;
        }

        return JsonConvert.SerializeObject(obj, options);
    }

    /// <summary>
    /// Returns deserialized string using default <see cref="JsonSerializerSettings"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T DeserializeJson<T>(this string value)
    {
        return value.DeserializeJson<T>(new JsonSerializerSettings());
    }

    /// <summary>
    /// Returns deserialized string using custom <see cref="JsonSerializerSettings"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static T DeserializeJson<T>(this string value, JsonSerializerSettings settings)
    {
        if (DeserializeJson(value, typeof(T), settings) is T deserialized)
        {
            return deserialized;
        }

        return default(T);
    }

    /// <summary>
    /// Returns deserialized string using explicit <see cref="Type"/> and custom <see cref="JsonSerializerSettings"/>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static object DeserializeJson(this string value, Type type, JsonSerializerSettings settings)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return value != null
                   ? JsonConvert.DeserializeObject(value, type, settings)
                   : null;
    }
}