namespace DotNetBrightener.PushNotification.FirebaseIntegration.Models;

public class PushNotificationMessageWrapper
{
    public PushNotificationMessageModel MessageModel { get; set; }
}


public class PushNotificationMessageModel
{
    public string                     topic        { get; set; } = null;
    public string                     token        { get; set; }
    public Notification               notification { get; set; }
    public Dictionary<string, object> data         { get; set; }
}

public class Notification
{
    public string title { get; set; }

    public string body  { get; set; }
}