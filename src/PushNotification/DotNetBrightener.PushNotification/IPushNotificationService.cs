namespace DotNetBrightener.PushNotification;

public interface IPushNotificationService
{
    Task DeliverNotificationToAllUsers(PushNotificationPayload pushNotification);

    Task DeliverNotificationToUsers(long[] userIds, PushNotificationPayload pushNotification);

    Task RegisterSubscriptionAsync(PushNotificationSubscription subscription);

    Task UnregisterSubscriptionAsync(long userId, string deviceToken);

    Task UnregisterAllSubscriptionsForUserAsync(long userId);
}



public interface IPushNotificationProvider
{
    string PushNotificationType { get; }

    Task DeliverNotification(string[] deviceToken, PushNotificationPayload payload);
}