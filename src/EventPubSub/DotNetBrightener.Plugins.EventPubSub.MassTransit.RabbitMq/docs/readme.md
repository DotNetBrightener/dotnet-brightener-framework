# RabbitMq with MassTransit Integration for [Event PubSub Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub)


&copy; 2024 [DotNet Brightener](mailto:admin@dotnetbrightener.com)


### Versions
| Library | Version |
| --- | --- |
| EventPubSub Core  |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub) |
| MassTransit Integration Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.MassTransit) |
| Dependency Injection Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.DependencyInjection) |
| Azure Service Bus with MassTransit Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.MassTransit.AzureServiceBus) |
| RabbitMq with MassTransit Library |![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.MassTransit.RabbitMq) |


## Installation

```powershell
dotnet package add DotNetBrightener.Plugins.EventPubSub
dotnet package add DotNetBrightener.Plugins.EventPubSub.DependencyInjection
dotnet package add DotNetBrightener.Plugins.EventPubSub.MassTransit
dotnet package add DotNetBrightener.Plugins.EventPubSub.MassTransit.AzureServiceBus
# or 
dotnet add package DotNetBrightener.Plugins.EventPubSub.MassTransit.RabbitMq
```

## Usage 

### Configuration

#### Option 1: Use `IConfiguration` to configure the service bus

```csharp

// Initialize EventPubSubService
var eventPubSubConfig = builder.Services
                               .AddEventPubSubService()
                                // scan for event messages in the given assembly
                               .AddEventMessagesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(DistributedTestMessage).Assembly)
                                // scan for event handlers in the given assembly
                               .AddEventHandlersFromAssemblies(Assembly.GetExecutingAssembly());

var massTransitConfigurator = eventPubSubConfig.EnableMassTransit();

// Add RabbitMq
massTransitConfigurator.UseRabbitMq(builder.Configuration);

// this should be called before builder.Build(), or before exiting ConfigureServices method
massTransitConfigurator.Finalize();
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

var massTransitConfigurator = eventPubSubConfig.EnableMassTransit();

massTransitConfigurator.SetSubscriptionName("<application-name>");
// if you want to exclude namespace in entity name
massTransitConfigurator.ExcludeNamespaceInEntityName();

// Add RabbitMq
massTransitConfigurator.UseRabbitMq("<rabbit-mq-host>",
                                    "<rabbit-mq-username>",
                                    "<rabbit-mq-password>",
                                    "<rabbit-mq-virtual-host>");

// this should be called before builder.Build(), or before exiting ConfigureServices method
massTransitConfigurator.Finalize();
```




Refer to the [Event PubSub Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub), [MassTransit Integration Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub.MassTransit) for more details on how to configure the event messages, request/response messages and how to implement handlers for them.