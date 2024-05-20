using System.Text.Json.Serialization;
using DotNetBrightener.Utils.MessageCompression;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DotNetBrightener.SecuredApi;

public class ApiMessage
{
    [JsonExtensionData]
    public Dictionary<string, object> Payload { get; set; } = null!;

    public static ApiMessage FromPayload(object payload)
    {
        return new ApiMessage
        {
            Payload = payload.ToPayload()
        };
    }

    public TOutput GetEntity<TOutput>() where TOutput : class, new()
    {
        var jsonPayload = JsonSerializer.Serialize(Payload, JsonSerializerSettings.SerializeOptions);

        TOutput result = JsonSerializer.Deserialize<TOutput>(jsonPayload, JsonSerializerSettings.DeserializeOptions);

        return result;
    }

    public object GetEntity(Type entityType)
    {
        var jsonPayload = JsonSerializer.Serialize(Payload, JsonSerializerSettings.SerializeOptions);

        return JsonSerializer.Deserialize(jsonPayload, entityType, JsonSerializerSettings.DeserializeOptions);
    }
}