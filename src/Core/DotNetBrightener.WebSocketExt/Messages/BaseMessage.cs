using DotNetBrightener.Utils.MessageCompression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetBrightener.WebSocketExt.Messages;

public abstract class BaseMessage
{
    /// <summary>
    ///     The identifier of the connection which responses the message
    /// </summary>
    public string ConnectionId { get; set; } = null!;

    /// <summary>
    ///     The identifier of the message. If the value is created by the client, it'll be used for the response too.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Describes the action information of the message.
    ///     If the value is from the request, it'll be used to determine which service to execute to handle the websocket request
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    ///     The payload of the message
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> Payload { get; set; } = null!;

    public TOutput? PayloadAs<TOutput>()
    {
        var jsonPayload = JsonSerializer.Serialize(this.Payload, JsonSerializerSettings.SerializeOptions);

        TOutput? result = JsonSerializer.Deserialize<TOutput>(jsonPayload, JsonSerializerSettings.DeserializeOptions);

        return result;
    }
}