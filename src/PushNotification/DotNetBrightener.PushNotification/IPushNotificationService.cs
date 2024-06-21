namespace DotNetBrightener.PushNotification;

public interface IPushNotificationService
{
    Task DeliverNotificationToAllUsers(PushNotificationPayload pushNotification);

    Task DeliverNotificationToUsers(long[] userIds, PushNotificationPayload pushNotification);
}



public interface IPushNotificationProvider
{
    string PushNotificationType { get; }

    Task DeliverNotification(string[] deviceToken, PushNotificationPayload payload);
}