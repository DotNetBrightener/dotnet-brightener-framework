# DotNetBrightener Push Notification Library

A comprehensive push notification library for .NET applications that supports multiple providers including Apple Push Notification service (APNs) and Firebase Cloud Messaging (FCM).

## Features

- **Multi-Provider Support**: Seamlessly integrate with APNs and FCM
- **Dependency Injection**: Built-in DI support with configuration builders
- **Subscription Management**: Handle user device subscriptions with pluggable repositories
- **Async/Await**: Full async support for high-performance applications
- **Logging**: Comprehensive logging with Microsoft.Extensions.Logging
- **Error Handling**: Robust error handling and retry mechanisms
- **Platform Detection**: Automatic provider selection based on device platform

## Quick Start

### 1. Installation

Install the required packages:

```bash
# Core library
dotnet add package DotNetBrightener.PushNotification

# Dependency injection support
dotnet add package DotNetBrightener.PushNotification.DependencyInjection

# Provider packages
dotnet add package DotNetBrightener.PushNotification.APN
dotnet add package DotNetBrightener.PushNotification.FirebaseIntegration
```

### 2. Configuration

Add the following to your `appsettings.json`:

```json
{
  "PushNotification": {
    "Apple": {
      "TeamId": "YOUR_TEAM_ID",
      "KeyId": "YOUR_KEY_ID",
      "PrivateKey": "YOUR_BASE64_ENCODED_PRIVATE_KEY",
      "BundleId": "com.yourcompany.yourapp",
      "UseSandbox": true
    },
    "Firebase": {
      "ProjectId": "your-firebase-project-id",
      "ServiceKey": "YOUR_FIREBASE_SERVICE_ACCOUNT_JSON",
      "SenderId": "your-sender-id",
      "EnableForIos": false
    }
  }
}
```

### 3. Service Registration

In your `Program.cs` or `Startup.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register push notification services
builder.Services.AddPushNotification(builder.Configuration)
    .AddApplePushNotification(builder.Configuration)
    .AddFirebaseCloudMessaging(builder.Configuration);
```

### 4. Basic Usage

```csharp
public class NotificationController : ControllerBase
{
    private readonly IPushNotificationService _pushNotificationService;

    public NotificationController(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        var payload = new PushNotificationPayload
        {
            Title = request.Title,
            Body = request.Body,
            Data = request.CustomData
        };

        await _pushNotificationService.DeliverNotificationToUsers(request.UserIds, payload);
        
        return Ok();
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var subscription = new PushNotificationSubscription
        {
            UserId = request.UserId,
            DeviceToken = request.DeviceToken,
            DevicePlatform = request.Platform // "ios", "android", "web"
        };

        await _pushNotificationService.RegisterSubscriptionAsync(subscription);
        
        return Ok();
    }
}
```

## Advanced Configuration

### Custom Subscription Repository

By default, the library uses an in-memory subscription repository. For production use, implement your own:

```csharp
public class DatabaseSubscriptionRepository : IPushNotificationSubscriptionRepository
{
    // Implement methods to store subscriptions in your database
}

// Register your custom repository
builder.Services.AddPushNotification(builder.Configuration)
    .UseSubscriptionRepository<DatabaseSubscriptionRepository>()
    .AddApplePushNotification(builder.Configuration)
    .AddFirebaseCloudMessaging(builder.Configuration);
```

### Configuration via Code

You can also configure providers programmatically:

```csharp
builder.Services.AddPushNotification()
    .AddApplePushNotification(options =>
    {
        options.TeamId = "YOUR_TEAM_ID";
        options.KeyId = "YOUR_KEY_ID";
        options.PrivateKey = "YOUR_PRIVATE_KEY";
        options.BundleId = "com.yourcompany.yourapp";
        options.UseSandbox = false;
    })
    .AddFirebaseCloudMessaging(options =>
    {
        options.ProjectId = "your-project-id";
        options.ServiceKey = "your-service-key-json";
        options.SenderId = "your-sender-id";
    });
```

## Next Steps

- [Apple Push Notification Setup Guide](./apns-setup.md)
- [Firebase Cloud Messaging Setup Guide](./fcm-setup.md)
- [API Reference](./api-reference.md)
- [Testing Guide](./testing.md)
