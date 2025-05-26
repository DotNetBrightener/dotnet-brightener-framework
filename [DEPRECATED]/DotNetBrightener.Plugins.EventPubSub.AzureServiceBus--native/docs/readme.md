# Azure Service Bus Enabled for [Event PubSub Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub)


&copy; 2024 [DotNet Brightener](mailto:admin@dotnetbrightener.com)


### Versions
| Library | Version |
| --- | --- |
| EventPubSub Core  |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub) |
| Azure Service Bus Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.AzureServiceBus) |
| Dependency Injection Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.DependencyInjection) |


## Installation

```powershell
dotnet package add DotNetBrightener.Plugins.EventPubSub
dotnet package add DotNetBrightener.Plugins.EventPubSub.DependencyInjection
dotnet package add DotNetBrightener.Plugins.EventPubSub.AzureServiceBus
```

## Usage 

### Configuration

#### Option 1: Use `IConfiguration` to configure the service bus

```csharp

builder.Services
        // Initialize EventPubSubService
       .AddEventPubSubService(
                              Assembly.GetExecutingAssembly(),
                              // assemblies where the event messages are defined
                              typeof(DistributedTestMessage).Assembly
                             )
        // Add Azure Service Bus
       .AddAzureServiceBus(builder.Configuration)
        // Add event handlers
       .AddEventHandlersFromAssemblies([
            Assembly.GetExecutingAssembly(),
            // assemblies where the event handlers are defined
        ]);
```

In your `appsettings.json` file, add the the following configuration:

```json
"ServiceBusConfiguration": {
    "ConnectionString": "<connection_string_to_Azure_Service_Bus",
    "SubscriptionName": "<your_app_name>"
} 
```


#### Option 2: Configure from code

```csharp   

builder.Services
        // Initialize EventPubSubService
       .AddEventPubSubService(
                              Assembly.GetExecutingAssembly(),
                              // assemblies where the event messages are defined
                              typeof(DistributedTestMessage).Assembly
                             )
        // Add Azure Service Bus
       .AddAzureServiceBus("<connection_string_to_Azure_Service_Bus>",
                           "<your_app_name>")
        // Add event handlers
       .AddEventHandlersFromAssemblies([
            Assembly.GetExecutingAssembly(),
            // assemblies where the event handlers are defined
        ]);
```

#### Default Configuration Value

If you use `appsettings.json` to configure the service bus, you can also customize the following configuration values by adding it to the JSON:

| Configuration | Description | Value Type | Default Value |
| --- | --- | --- | --- |
| ConnectionString | The connection string to Azure Service Bus | string | `required`, must specify |
| SubscriptionName | The name of the application that subscribes to the events from Azure Service Bus | string | `required`, must specify |
| IncludeNamespaceForTopicName | Indicates if the namespace of the class is used for topic name | boolean | `true`, means the namespace will be used for topic name.<br />Set to `false` to only use class name as topic name |
| AutoDeleteOnIdle | The `TimeSpan` idle interval after which the topic is automatically deleted. | TimeSpan | `TimeSpan.FromDays(90)`, means the topic will be automatically deleted after 90 days. |
| DefaultMessageTimeToLive | The default time to live value for the messages. This is the duration after which the message expires, starting from when the message is sent to Service Bus. | TimeSpan | `TimeSpan.FromDays(5)` |
| DuplicateDetectionHistoryTimeWindow | The `TimeSpan` duration of duplicate detection history that is maintained by the service. | TimeSpan | `TimeSpan.FromMinutes(1)` |
| MaxSizeInMegabytes | The maximum size of the topic in megabytes, which is the size of memory allocated for the topic. | long | `5120` |

### Event Message Definition

Create a class that represents the event message. The class must implement the `IDistributedEventMessage` interface.

```csharp

namespace YourProject.NameSpace;

public class DistributedTestMessage : IDistributedEventMessage
{
    public string Name { get; set; }

    // more payload properties
}

```

### Event Handler Definition

Create a class derived from `DistributedEventEventHandler<YourEventMessage>` and override `HandleEvent` method to handle the event message.

With this implementation, you'll be able to access the original payload of the messsage by accessing `OriginPayload` property.

```csharp


public class TestEventHandlerDistributed(ILogger<TestEventHandlerDistributed> logger)
    : DistributedEventEventHandler<DistributedTestMessage>
{
    private readonly ILogger _logger = logger;
    
    public override Task<bool> HandleEvent(DistributedTestMessage eventMessage)
    {
        // access the `OriginPayload` 
        var origin = OriginPayload;

        _logger.LogInformation($"Received message: {eventMessage.Name}");

        return Task.FromResult<bool>(true);
    }
}

```