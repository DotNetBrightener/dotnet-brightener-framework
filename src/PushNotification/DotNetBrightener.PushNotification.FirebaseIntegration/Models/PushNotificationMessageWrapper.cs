namespace DotNetBrightener.PushNotification.FirebaseIntegration.Models;

internal class PushNotificationMessageWrapper
{
    public PushNotificationMessageModel MessageModel { get; set; }
}

internal class PushNotificationMessageModel
{
    public string                     topic        { get; set; } = null;
    public string                     token        { get; set; }
    public Notification               notification { get; set; }
    public Dictionary<string, object> data         { get; set; }
}

internal class Notification
{
    public string title { get; set; }

    public string body  { get; set; }
}