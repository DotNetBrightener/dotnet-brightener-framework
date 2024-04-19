namespace DotNetBrightener.WebSocketExt.Messages;

public class RequestMessage : BaseMessage
{
    /// <summary>
    ///     Creates a response message for this request using the provided payload data
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the payload
    /// </typeparam>
    /// <param name="payload"></param>
    /// <returns></returns>
    public ResponseMessage ResponseWithPayload<T>(T payload) where T : BasePayload
    {
        return new ResponseMessage
        {
            Id           = Id,
            ConnectionId = ConnectionId,
            Action       = Action,
            Payload      = payload.ToPayload<T>()
        };
    }

    /// <summary>
    ///     Responses with unauthorized error message
    /// </summary>
    /// <returns></returns>
    public ResponseMessage Unauthorized()
    {
        return new ResponseMessage
        {
            Id           = Id,
            ConnectionId = ConnectionId,
            Action       = Action,
            ErrorMessage = "Unauthorized access"
        };
    }
}