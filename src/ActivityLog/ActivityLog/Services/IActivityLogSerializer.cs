using System.Text.Json;
using System.Text.Json.Serialization;
using ActivityLog.Configuration;
using Microsoft.Extensions.Options;

namespace ActivityLog.Services;

/// <summary>
/// Interface for serializing objects for activity logging
/// </summary>
public interface IActivityLogSerializer
{
    /// <summary>
    /// Serializes a return value to a JSON string
    /// </summary>
    /// <param name="returnValue">The return value to serialize</param>
    /// <returns>Serialized return value as JSON string</returns>
    string SerializeReturnValue(object? returnValue);

    /// <summary>
    /// Serializes an exception to a JSON string
    /// </summary>
    /// <param name="exception">The exception to serialize</param>
    /// <returns>Serialized exception as JSON string</returns>
    string SerializeException(Exception exception);

    /// <summary>
    /// Serializes metadata to a JSON string
    /// </summary>
    /// <param name="metadata">The metadata dictionary to serialize</param>
    /// <returns>Serialized metadata as JSON string</returns>
    string SerializeMetadata(Dictionary<string, object?> metadata);
}

/// <summary>
/// Default implementation of IActivityLogSerializer using System.Text.Json
/// </summary>
public class ActivityLogSerializer : IActivityLogSerializer
{
    private readonly SerializationConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public ActivityLogSerializer(IOptions<ActivityLogConfiguration> configuration)
    {
        _config = configuration.Value.Serialization;
        _jsonOptions = CreateJsonSerializerOptions();
    }

    public string SerializeReturnValue(object? returnValue)
    {
        if (!_config.SerializeReturnValues || returnValue == null)
            return "null";

        try
        {
            if (ShouldExcludeType(returnValue.GetType()))
                return "[EXCLUDED]";

            var sanitizedValue = SanitizeValue(returnValue);
            return JsonSerializer.Serialize(sanitizedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            return $"[SERIALIZATION_ERROR: {ex.Message}]";
        }
    }

    public string SerializeException(Exception exception)
    {
        try
        {
            var exceptionInfo = new Dictionary<string, object?>
            {
                ["Type"] = exception.GetType().FullName,
                ["Message"] = TruncateString(exception.Message, _config.MaxStringLength),
                ["StackTrace"] = TruncateString(exception.StackTrace, _config.MaxStringLength * 2),
                ["Source"] = exception.Source,
                ["HelpLink"] = exception.HelpLink,
                ["Data"] = exception.Data.Count > 0 ? exception.Data : null
            };

            // Add inner exceptions if configured
            if (exception.InnerException != null)
            {
                exceptionInfo["InnerException"] = SerializeInnerException(exception.InnerException, 0);
            }

            return JsonSerializer.Serialize(exceptionInfo, _jsonOptions);
        }
        catch (Exception ex)
        {
            return $"[EXCEPTION_SERIALIZATION_ERROR: {ex.Message}]";
        }
    }

    public string SerializeMetadata(Dictionary<string, object?> metadata)
    {
        if (metadata.Count == 0)
            return "{}";

        try
        {
            var sanitizedMetadata = new Dictionary<string, object?>();
            foreach (var kvp in metadata)
            {
                if (ShouldExcludeProperty(kvp.Key))
                {
                    sanitizedMetadata[kvp.Key] = "[SENSITIVE]";
                }
                else
                {
                    sanitizedMetadata[kvp.Key] = SanitizeValue(kvp.Value);
                }
            }

            // Use metadata-specific JSON options without SafeObjectConverter to properly serialize dictionaries
            var metadataJsonOptions = CreateMetadataJsonSerializerOptions();
            return JsonSerializer.Serialize(sanitizedMetadata, metadataJsonOptions);
        }
        catch (Exception ex)
        {
            return $"[METADATA_SERIALIZATION_ERROR: {ex.Message}]";
        }
    }

    private JsonSerializerOptions CreateJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = _config.IgnoreNullValues ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            MaxDepth = _config.MaxDepth,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters =
            {
                new JsonStringEnumConverter(),
                new SafeObjectConverter(_config)
            }
        };
    }

    private JsonSerializerOptions CreateMetadataJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = _config.IgnoreNullValues ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            MaxDepth = _config.MaxDepth,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters =
            {
                new JsonStringEnumConverter()
                // Note: Intentionally excluding SafeObjectConverter to allow proper dictionary serialization
            }
        };
    }

    private object? SanitizeValue(object? value, int depth = 0)
    {
        if (value == null)
            return null;

        // Prevent infinite recursion and excessive depth
        if (depth >= _config.MaxDepth)
            return "[MAX_DEPTH_REACHED]";

        var valueType = value.GetType();

        // Check if type should be excluded
        if (ShouldExcludeType(valueType))
            return $"[EXCLUDED:{valueType.Name}]";

        // Handle primitive types and common value types
        if (valueType.IsPrimitive ||
            valueType == typeof(string) ||
            valueType == typeof(DateTime) ||
            valueType == typeof(DateTimeOffset) ||
            valueType == typeof(TimeSpan) ||
            valueType == typeof(Guid) ||
            valueType == typeof(decimal))
        {
            if (value is string str)
                return TruncateString(str, _config.MaxStringLength);
            return value;
        }

        // Handle collections
        if (value is System.Collections.IEnumerable enumerable && valueType != typeof(string))
        {
            var items = new List<object?>();
            int count = 0;
            foreach (var item in enumerable)
            {
                if (count >= 10) // Limit collection size
                {
                    items.Add("[TRUNCATED]");
                    break;
                }
                items.Add(SanitizeValue(item, depth + 1));
                count++;
            }
            return items;
        }

        // For complex objects, serialize their properties
        return SanitizeComplexObject(value, depth);
    }

    private object? SanitizeComplexObject(object value, int depth)
    {
        try
        {
            var valueType = value.GetType();

            // Create a dictionary to hold the sanitized properties
            var sanitizedObject = new Dictionary<string, object?>();

            // Get all public properties
            var properties = valueType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Skip properties that can't be read
                if (!property.CanRead)
                    continue;

                // Skip indexed properties (like this[int index])
                if (property.GetIndexParameters().Length > 0)
                    continue;

                var propertyName = property.Name;

                // Check if property should be excluded
                if (ShouldExcludeProperty(propertyName))
                {
                    sanitizedObject[propertyName] = "[SENSITIVE]";
                    continue;
                }

                try
                {
                    var propertyValue = property.GetValue(value);
                    sanitizedObject[propertyName] = SanitizeValue(propertyValue, depth + 1);
                }
                catch (Exception ex)
                {
                    // If we can't read the property value, mark it as an error
                    sanitizedObject[propertyName] = $"[PROPERTY_ERROR: {ex.Message}]";
                }
            }

            return sanitizedObject;
        }
        catch (Exception ex)
        {
            // If we can't serialize the object at all, fall back to type name
            return $"[SERIALIZATION_ERROR: {ex.Message}]";
        }
    }

    private Dictionary<string, object?> SerializeInnerException(Exception exception, int depth)
    {
        if (depth >= 3) // Limit inner exception depth
            return new Dictionary<string, object?> { ["Message"] = "[MAX_DEPTH_REACHED]" };

        var innerExceptionInfo = new Dictionary<string, object?>
        {
            ["Type"] = exception.GetType().FullName,
            ["Message"] = TruncateString(exception.Message, _config.MaxStringLength),
            ["StackTrace"] = TruncateString(exception.StackTrace, _config.MaxStringLength)
        };

        if (exception.InnerException != null)
        {
            innerExceptionInfo["InnerException"] = SerializeInnerException(exception.InnerException, depth + 1);
        }

        return innerExceptionInfo;
    }

    private bool ShouldExcludeType(Type type)
    {
        var typeName = type.FullName ?? type.Name;
        return _config.ExcludedTypes.Any(excludedType => 
            typeName.Contains(excludedType, StringComparison.OrdinalIgnoreCase));
    }

    private bool ShouldExcludeProperty(string propertyName)
    {
        return _config.ExcludedProperties.Any(excludedProp => 
            propertyName.Contains(excludedProp, StringComparison.OrdinalIgnoreCase));
    }

    private static string TruncateString(string? input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input ?? string.Empty;

        return input.Substring(0, maxLength - 3) + "...";
    }
}

/// <summary>
/// Custom JSON converter that safely handles complex objects
/// </summary>
public class SafeObjectConverter(SerializationConfiguration config) : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return !typeToConvert.IsPrimitive && 
               typeToConvert != typeof(string) && 
               typeToConvert != typeof(DateTime) &&
               typeToConvert != typeof(DateTimeOffset);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization is not supported");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        try
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var type = value.GetType();
            if (type.FullName != null && config.ExcludedTypes.Any(et => type.FullName.Contains(et)))
            {
                writer.WriteStringValue($"[EXCLUDED:{type.Name}]");
                return;
            }

            // For complex objects, write a simple representation
            writer.WriteStringValue($"[{type.Name}]");
        }
        catch
        {
            writer.WriteStringValue("[SERIALIZATION_ERROR]");
        }
    }
}
