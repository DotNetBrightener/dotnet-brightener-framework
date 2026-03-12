namespace DotNetBrightener.PushNotification;

public interface IPushNotificationSubscriptionRepository
{
    Task<IEnumerable<PushNotificationSubscription>> GetSubscriptionsForUsersAsync(long[] userIds);
    Task<IEnumerable<PushNotificationSubscription>> GetAllSubscriptionsAsync();
    Task<PushNotificationSubscription> GetSubscriptionAsync(long userId, string deviceToken);
    Task AddOrUpdateSubscriptionAsync(PushNotificationSubscription subscription);
    Task RemoveSubscriptionAsync(long userId, string deviceToken);
    Task RemoveAllSubscriptionsForUserAsync(long userId);
}
