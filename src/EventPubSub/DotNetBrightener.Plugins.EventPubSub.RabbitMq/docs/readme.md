# RabbitMq Integration for [Event PubSub Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub)


&copy; 2024 [DotNet Brightener](mailto:admin@dotnetbrightener.com)


### Versions
| Library | Version |
| --- | --- |
| EventPubSub Core  |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub) |
| EventPubSub Distributed Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.Distributed) |
| Dependency Injection Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.DependencyInjection) |
| Azure Service Bus Integration Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.Distributed.AzureServiceBus) |
| RabbitMq Integration Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.Distributed.RabbitMq) |


## Installation

```powershell
dotnet package add DotNetBrightener.Plugins.EventPubSub
dotnet package add DotNetBrightener.Plugins.EventPubSub.DependencyInjection
dotnet package add DotNetBrightener.Plugins.EventPubSub.Distributed
dotnet package add DotNetBrightener.Plugins.EventPubSub.AzureServiceBus
# or 
dotnet add package DotNetBrightener.Plugins.EventPubSub.RabbitMq
```

## Usage 

### Configuration

#### Option 1: Use `IConfiguration` to configure the service bus

```csharp

// Initialize EventPubSubService
var eventPubSubConfig = builder.Services
                               .AddEventPubSubService()
                                // scan for event messages in the given assembly
                               .AddEventMessagesFromAssemblies(typeof(DistributedTestMessage).Assembly)
                                // scan for event handlers in the given assembly
                               .AddEventHandlersFromAssemblies(Assembly.GetExecutingAssembly());

// Add RabbitMq
eventPubSubConfig.UseRabbitMq(builder.Configuration)
                 .Finalize();
```

In your `appsettings.json` file, add the the following configuration:

```json
  "RabbitMqConfiguration": {
    "Host": "<rabbit-mq-host>",
    "Username": "<rabbit-mq-username>",
    "Password": "<rabbit-mq-password>",
    "VirtualHost": "<rabbit-mq-virtual-host>",
    "SubscriptionName": "<application-name>",
    "IncludeNamespaceForTopicName": false
  }
```


#### Option 2: Configure from code

```csharp   

// Initialize EventPubSubService
var eventPubSubConfig = builder.Services
                               .AddEventPubSubService()
                                // scan for event messages in the given assembly
                               .AddEventMessagesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(DistributedTestMessage).Assembly)
                                // scan for event handlers in the given assembly
                               .AddEventHandlersFromAssemblies(Assembly.GetExecutingAssembly());

var distributedIntegrations = eventPubSubConfig.EnableDistributedIntegrations();

distributedIntegrations.SetSubscriptionName("<application-name>");
// if you want to exclude namespace in entity name
distributedIntegrations.ExcludeNamespaceInEntityName();

// Add RabbitMq
distributedIntegrations.UseRabbitMq("<rabbit-mq-host>",
                                    "<rabbit-mq-username>",
                                    "<rabbit-mq-password>",
                                    "<rabbit-mq-virtual-host>");

// this should be called before builder.Build(), or before exiting ConfigureServices method
distributedIntegrations.Finalize();
```




Refer to the [Event PubSub Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub), [MassTransit Integration Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub.Distributed) for more details on how to configure the event messages, request/response messages and how to implement handlers for them.