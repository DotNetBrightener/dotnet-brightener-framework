namespace DotNetBrightener.PushNotification;

public class PushNotificationPayload
{
    public long? TargetUserId { get; set; }

    public long[] TargetUserIds { get; set; } = [];

    public string Title { get; set; }

    public string Body { get; set; }

    public Dictionary<string, object> Data { get; set; }
}