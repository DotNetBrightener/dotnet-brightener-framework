using DotNetBrightener.WebSocketExt.Internal;
using System.Text.Json;

namespace DotNetBrightener.WebSocketExt.Messages;

public abstract class BasePayload
{
    public abstract string Action { get; }

    public Dictionary<string, object?> ToPayload<T>() where T : BasePayload
    {
        var jsonPayload = JsonSerializer.Serialize((T)this, JsonSerializerSettings.SerializeOptions);

        Dictionary<string, object?> result =
            JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonPayload, JsonSerializerSettings.DeserializeOptions)!;

        result.Remove(nameof(Action).ToLower());

        return result;
    }

    public TOutput ToPayload<T, TOutput>() where T : BasePayload
    {
        var jsonPayload = JsonSerializer.Serialize((T)this, JsonSerializerSettings.SerializeOptions);

        TOutput result = JsonSerializer.Deserialize<TOutput>(jsonPayload, JsonSerializerSettings.DeserializeOptions)!;

        return result;
    }

    public static T FromPayload<T>(Dictionary<string, object?> payload) where T : BasePayload
    {
        var jsonPayload = JsonSerializer.Serialize(payload, JsonSerializerSettings.SerializeOptions);

        var instance = JsonSerializer.Deserialize<T>(jsonPayload, JsonSerializerSettings.DeserializeOptions)!;

        return instance;
    }

    protected string GetActionName()
    {
        return GetType().Name.Replace("Payload", "");
    }
}