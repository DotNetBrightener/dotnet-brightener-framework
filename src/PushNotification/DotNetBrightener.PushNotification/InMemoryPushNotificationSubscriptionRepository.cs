using System.Collections.Concurrent;

namespace DotNetBrightener.PushNotification;

public class InMemoryPushNotificationSubscriptionRepository : IPushNotificationSubscriptionRepository
{
    private readonly ConcurrentDictionary<string, PushNotificationSubscription> _subscriptions = new();

    public Task<IEnumerable<PushNotificationSubscription>> GetSubscriptionsForUsersAsync(long[] userIds)
    {
        var subscriptions = _subscriptions.Values
            .Where(s => userIds.Contains(s.UserId))
            .ToList();

        return Task.FromResult<IEnumerable<PushNotificationSubscription>>(subscriptions);
    }

    public Task<IEnumerable<PushNotificationSubscription>> GetAllSubscriptionsAsync()
    {
        return Task.FromResult<IEnumerable<PushNotificationSubscription>>(_subscriptions.Values.ToList());
    }

    public Task<PushNotificationSubscription> GetSubscriptionAsync(long userId, string deviceToken)
    {
        var key = GetKey(userId, deviceToken);
        _subscriptions.TryGetValue(key, out var subscription);
        return Task.FromResult(subscription);
    }

    public Task AddOrUpdateSubscriptionAsync(PushNotificationSubscription subscription)
    {
        var key = GetKey(subscription.UserId, subscription.DeviceToken);
        _subscriptions.AddOrUpdate(key, subscription, (_, _) => subscription);
        return Task.CompletedTask;
    }

    public Task RemoveSubscriptionAsync(long userId, string deviceToken)
    {
        var key = GetKey(userId, deviceToken);
        _subscriptions.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveAllSubscriptionsForUserAsync(long userId)
    {
        var keysToRemove = _subscriptions
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _subscriptions.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    private static string GetKey(long userId, string deviceToken)
    {
        return $"{userId}:{deviceToken}";
    }
}
