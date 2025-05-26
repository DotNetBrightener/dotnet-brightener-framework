# Distributed Integration for [Event PubSub Library](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub)


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

// Add Azure Service Bus
//eventPubSubConfig.UseAzureServiceBus(builder.Configuration)
//                 .Finalize();

// Add RabbitMq
eventPubSubConfig.UseRabbitMq(builder.Configuration)
                 .Finalize();
```

In your `appsettings.json` file, add the the following configuration:

```json
  "ServiceBusConfiguration": {
    "ConnectionString": "<your-endpoint-to-azure-service-bus>",
    "SubscriptionName": "<application-name>",
    "IncludeNamespaceForTopicName": false
  },
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

// Add Azure Service Bus
distributedIntegrations.UseAzureServiceBus("<azure-connection-string>");

// Or Add RabbitMq - Note that only one or the other can be used
/*
distributedIntegrations.UseRabbitMq("<rabbit-mq-host>",
                                    "<rabbit-mq-username>",
                                    "<rabbit-mq-password>",
                                    "<rabbit-mq-virtual-host>");
*/

// this should be called before builder.Build(), or before exiting ConfigureServices method
distributedIntegrations.Finalize();
```


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

### Request - Response Message Pattern

Create 2 (two) classes that represents the request message and the response message. The request message class must implement the `IRequestMessage` interface, while the response message class must implement the `IResponseMessage<T>` interface, where `T` is the request message type.

```csharp

namespace YourProject.NameSpace;

public class RequestMessage : IRequestMessage
{
    public string Name { get; set; }

    // more payload properties
}

public class ResponseMessage : IResponseMessage<RequestMessage>
{
    public string Name { get; set; }

    // more payload properties
}

```

When you want to initiate a request to obtain the needed response, use IEventPublisher to initiate it as follow:

```csharp

var requestMessage = new RequestMessage
{
    Name = eventMessage.Name
};

var response = await eventPublisher.GetResponse<ResponseMessage, RequestMessage>(requestMessage);

```

### Request - Response Handler Definition

Create a class derived from `RequestResponder<YourRequestMessage>` and override `HandleEvent` method to handle the event message.


```csharp


public class HandleRequestMessage(ILogger<HandleRequestMessage> logger)
    : RequestResponder<RequestMessage>
{    
    public override async Task<bool> HandleEvent(RequestMessage eventMessage)
    {
        // do something with the request message, e.g. logging it
        logger.LogInformation($"Received message: {eventMessage.Name}");

        // create a response message - this response message must implement IResponseMessage<T> interface
        var responseMessage = new ResponseMessage
        {
            Name = eventMessage.Name
        };

        // send the response message
        await SendResponse(responseMessage);

        return Task.FromResult<bool>(true);
    }
}

```