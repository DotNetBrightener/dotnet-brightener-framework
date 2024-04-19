namespace DotNetBrightener.WebSocketExt.Messages;

internal class ConnectedNotificationPayload : BasePayload
{
    public override string Action => "ConnectedNotification";
}

internal class CommonResponsePayload : BasePayload
{
    public override string Action { get; }

    public CommonResponsePayload(string action)
    {
        Action = action;
    }
}