using Microsoft.Extensions.Logging;

namespace DotNetBrightener.PushNotification;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushNotificationProviderFactory _providerFactory;
    private readonly IPushNotificationSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IPushNotificationProviderFactory providerFactory,
        IPushNotificationSubscriptionRepository subscriptionRepository,
        ILogger<PushNotificationService> logger)
    {
        _providerFactory = providerFactory;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task DeliverNotificationToAllUsers(PushNotificationPayload pushNotification)
    {
        try
        {
            _logger.LogInformation("Delivering notification to all users: {Title}", pushNotification.Title);

            var subscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            await DeliverNotificationToSubscriptions(subscriptions, pushNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering notification to all users");
            throw;
        }
    }

    public async Task DeliverNotificationToUsers(long[] userIds, PushNotificationPayload pushNotification)
    {
        try
        {
            _logger.LogInformation("Delivering notification to {UserCount} users: {Title}", 
                userIds.Length, pushNotification.Title);

            var subscriptions = await _subscriptionRepository.GetSubscriptionsForUsersAsync(userIds);
            await DeliverNotificationToSubscriptions(subscriptions, pushNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering notification to users {UserIds}", string.Join(",", userIds));
            throw;
        }
    }

    private async Task DeliverNotificationToSubscriptions(
        IEnumerable<PushNotificationSubscription> subscriptions, 
        PushNotificationPayload pushNotification)
    {
        var subscriptionGroups = subscriptions
            .GroupBy(s => s.DevicePlatform)
            .ToList();

        foreach (var group in subscriptionGroups)
        {
            var provider = _providerFactory.GetProviderForPlatform(group.Key);
            if (provider == null)
            {
                _logger.LogWarning("No provider found for platform: {Platform}", group.Key);
                continue;
            }

            var deviceTokens = group.Select(s => s.DeviceToken).ToArray();
            
            try
            {
                await provider.DeliverNotification(deviceTokens, pushNotification);
                _logger.LogInformation("Successfully delivered notification via {Provider} to {DeviceCount} devices", 
                    provider.PushNotificationType, deviceTokens.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver notification via {Provider} to {DeviceCount} devices", 
                    provider.PushNotificationType, deviceTokens.Length);
            }
        }
    }

    public async Task RegisterSubscriptionAsync(PushNotificationSubscription subscription)
    {
        try
        {
            _logger.LogInformation("Registering subscription for user {UserId} on platform {Platform}",
                subscription.UserId, subscription.DevicePlatform);

            await _subscriptionRepository.AddOrUpdateSubscriptionAsync(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering subscription for user {UserId}", subscription.UserId);
            throw;
        }
    }

    public async Task UnregisterSubscriptionAsync(long userId, string deviceToken)
    {
        try
        {
            _logger.LogInformation("Unregistering subscription for user {UserId} with device token {DeviceToken}",
                userId, deviceToken);

            await _subscriptionRepository.RemoveSubscriptionAsync(userId, deviceToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task UnregisterAllSubscriptionsForUserAsync(long userId)
    {
        try
        {
            _logger.LogInformation("Unregistering all subscriptions for user {UserId}", userId);

            await _subscriptionRepository.RemoveAllSubscriptionsForUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering all subscriptions for user {UserId}", userId);
            throw;
        }
    }
}
