namespace DotNetBrightener.WebSocketExt.Messages;

/// <summary>
///     Represents the message to response from websocket service
/// </summary>
public class ResponseMessage : BaseMessage
{
    public string? ErrorMessage { get; set; }

    public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);

    public static ResponseMessage FromPayload<T>(string  connectionId,
                                                 string  id,
                                                 T       basePayload,
                                                 string? errorMessage = null)
        where T : BasePayload
    {
        return new()
        {
            ConnectionId = connectionId,
            Id           = id,
            Action       = basePayload.Action,
            ErrorMessage = errorMessage,
            Payload      = basePayload.ToPayload<T>()
        };
    }
}