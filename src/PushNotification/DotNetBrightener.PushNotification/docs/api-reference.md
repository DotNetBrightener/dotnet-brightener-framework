# API Reference

## Core Interfaces

### IPushNotificationService

The main service interface for sending push notifications and managing subscriptions.

```csharp
public interface IPushNotificationService
{
    Task DeliverNotificationToAllUsers(PushNotificationPayload pushNotification);
    Task DeliverNotificationToUsers(long[] userIds, PushNotificationPayload pushNotification);
    Task RegisterSubscriptionAsync(PushNotificationSubscription subscription);
    Task UnregisterSubscriptionAsync(long userId, string deviceToken);
    Task UnregisterAllSubscriptionsForUserAsync(long userId);
}
```

#### Methods

**DeliverNotificationToAllUsers**
- Sends a notification to all registered users
- Parameters: `PushNotificationPayload pushNotification`
- Returns: `Task`

**DeliverNotificationToUsers**
- Sends a notification to specific users
- Parameters: `long[] userIds`, `PushNotificationPayload pushNotification`
- Returns: `Task`

**RegisterSubscriptionAsync**
- Registers a new device subscription for push notifications
- Parameters: `PushNotificationSubscription subscription`
- Returns: `Task`

**UnregisterSubscriptionAsync**
- Removes a specific device subscription
- Parameters: `long userId`, `string deviceToken`
- Returns: `Task`

**UnregisterAllSubscriptionsForUserAsync**
- Removes all subscriptions for a user
- Parameters: `long userId`
- Returns: `Task`

### IPushNotificationProvider

Interface for implementing push notification providers (APNs, FCM, etc.).

```csharp
public interface IPushNotificationProvider
{
    string PushNotificationType { get; }
    Task DeliverNotification(string[] deviceToken, PushNotificationPayload payload);
}
```

### IPushNotificationSubscriptionRepository

Interface for managing push notification subscriptions storage.

```csharp
public interface IPushNotificationSubscriptionRepository
{
    Task<IEnumerable<PushNotificationSubscription>> GetSubscriptionsForUsersAsync(long[] userIds);
    Task<IEnumerable<PushNotificationSubscription>> GetAllSubscriptionsAsync();
    Task<PushNotificationSubscription> GetSubscriptionAsync(long userId, string deviceToken);
    Task AddOrUpdateSubscriptionAsync(PushNotificationSubscription subscription);
    Task RemoveSubscriptionAsync(long userId, string deviceToken);
    Task RemoveAllSubscriptionsForUserAsync(long userId);
}
```

## Models

### PushNotificationPayload

Represents the notification content to be sent.

```csharp
public class PushNotificationPayload
{
    public long? TargetUserId { get; set; }
    public long[] TargetUserIds { get; set; } = [];
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, object> Data { get; set; }
}
```

### PushNotificationSubscription

Represents a user's device subscription.

```csharp
public class PushNotificationSubscription
{
    public long UserId { get; set; }
    public string DeviceToken { get; set; }
    public string DevicePlatform { get; set; } // "ios", "android", "web"
}
```

## Configuration Classes

### ApnSettings

Configuration for Apple Push Notification service.

```csharp
public class ApnSettings
{
    public string TeamId { get; set; }
    public string KeyId { get; set; }
    public string PrivateKey { get; set; }
    public string BundleId { get; set; }
    public bool UseSandbox { get; set; } = true;
    public string ApnServerUrl { get; } // Computed property
}
```

### PushNotificationFirebaseSettings

Configuration for Firebase Cloud Messaging.

```csharp
public class PushNotificationFirebaseSettings
{
    public string ProjectId { get; set; }
    public string ServiceKey { get; set; }
    public string SenderId { get; set; }
    public bool EnableForIos { get; set; }
}
```

## Extension Methods

### ServiceCollectionExtensions

```csharp
// Add push notification services
public static PushNotificationConfigurationBuilder AddPushNotification(
    this IServiceCollection serviceCollection,
    IConfiguration configuration = null)

// Add Firebase Cloud Messaging provider
public static PushNotificationConfigurationBuilder AddFirebaseCloudMessaging(
    this PushNotificationConfigurationBuilder builder,
    IConfiguration configuration,
    string configurationSectionName = "PushNotification:Firebase")

// Add Apple Push Notification provider
public static PushNotificationConfigurationBuilder AddApplePushNotification(
    this PushNotificationConfigurationBuilder builder,
    IConfiguration configuration,
    string configurationSectionName = "PushNotification:Apple")

// Use custom subscription repository
public static PushNotificationConfigurationBuilder UseSubscriptionRepository<TRepository>(
    this PushNotificationConfigurationBuilder builder)
    where TRepository : class, IPushNotificationSubscriptionRepository
```

## Constants

### PushNotificationEndpointType

```csharp
public static class PushNotificationEndpointType
{
    public const string Ios = "ApplePushNotification";
    public const string FirebaseCloudMessaging = "FirebaseCloudMessaging";
}
```

## Usage Examples

### Sending Notifications

```csharp
// Send to specific users
var payload = new PushNotificationPayload
{
    Title = "New Message",
    Body = "You have a new message",
    Data = new Dictionary<string, object>
    {
        ["messageId"] = "12345",
        ["type"] = "chat"
    }
};

await pushNotificationService.DeliverNotificationToUsers(
    new long[] { 1, 2, 3 }, 
    payload);

// Send to all users
await pushNotificationService.DeliverNotificationToAllUsers(payload);
```

### Managing Subscriptions

```csharp
// Register a new subscription
var subscription = new PushNotificationSubscription
{
    UserId = 123,
    DeviceToken = "device-token-here",
    DevicePlatform = "ios"
};

await pushNotificationService.RegisterSubscriptionAsync(subscription);

// Unregister a subscription
await pushNotificationService.UnregisterSubscriptionAsync(123, "device-token-here");

// Unregister all subscriptions for a user
await pushNotificationService.UnregisterAllSubscriptionsForUserAsync(123);
```
