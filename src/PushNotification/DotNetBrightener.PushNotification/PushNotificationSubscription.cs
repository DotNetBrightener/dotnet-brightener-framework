namespace DotNetBrightener.PushNotification;

public class PushNotificationSubscription
{
    public long UserId { get; set; }

    public string DeviceToken { get; set; }

    public string DevicePlatform { get; set; }
}