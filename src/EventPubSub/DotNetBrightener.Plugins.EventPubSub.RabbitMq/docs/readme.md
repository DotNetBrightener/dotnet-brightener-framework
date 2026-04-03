# DotNetBrightener.Plugins.EventPubSub.RabbitMq

RabbitMQ provider for the DotNetBrightener EventPubSub system. Provides distributed event publishing and subscribing capabilities using RabbitMQ as the message broker.

## Installation

```bash
dotnet add package DotNetBrightener.Plugins.EventPubSub.RabbitMq
```

## Configuration

### appsettings.json

```json
{
  "RabbitMqConfiguration": {
    "HostName": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "UserName": "guest",
    "Password": "guest",
    "SubscriptionName": "my-service",
    "IncludeNamespaceForExchangeName": true,
    "DurableExchanges": true,
    "DurableQueues": true,
    "ResponseTimeout": "00:02:00"
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `HostName` | `string` | _(required)_ | RabbitMQ server hostname |
| `Port` | `int` | `5672` | RabbitMQ server port |
| `VirtualHost` | `string` | `"/"` | Virtual host |
| `UserName` | `string` | `"guest"` | Authentication username |
| `Password` | `string` | `"guest"` | Authentication password |
| `SubscriptionName` | `string` | _(required)_ | Unique name for this subscriber. Used as queue prefix. |
| `IncludeNamespaceForExchangeName` | `bool` | `true` | Use full type name (including namespace) as exchange name |
| `DurableExchanges` | `bool` | `true` | Exchanges survive broker restart |
| `DurableQueues` | `bool` | `true` | Queues survive broker restart |
| `AutoDeleteExchanges` | `bool` | `false` | Auto-delete exchanges when unused |
| `ResponseTimeout` | `TimeSpan` | `2 min` | Timeout for request-response operations |

## Usage

### 1. Define Event Messages

```csharp
using DotNetBrightener.Plugins.EventPubSub;

public class UserCreatedMessage : DistributedEventMessage
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
}
```

### 2. Define Event Handlers

```csharp
public class UserCreatedHandler : DistributedEventHandler<UserCreatedMessage>
{
    public override int Priority => 100;

    public override async Task HandleEvent(UserCreatedMessage eventMessage)
    {
        // Handle the event
    }
}
```

### 3. Register Services

```csharp
// In Program.cs / Startup.cs
services.AddEventPubSubService()
        .AddEventMessagesFromAssemblies(typeof(UserCreatedMessage).Assembly)
        .AddEventHandlersFromAssemblies(typeof(UserCreatedHandler).Assembly)
        .AddRabbitMq("localhost", subscriptionName: "my-service");
```

Or with `IConfiguration`:

```csharp
services.AddEventPubSubService()
        .AddEventMessagesFromAssemblies(typeof(UserCreatedMessage).Assembly)
        .AddEventHandlersFromAssemblies(typeof(UserCreatedHandler).Assembly)
        .AddRabbitMq(configuration, subscriptionName: "my-service");
```

### 4. Publish Events

```csharp
public class SomeService(IEventPublisher eventPublisher)
{
    public async Task CreateUser(User user)
    {
        // ... create user logic ...

        await eventPublisher.Publish(new UserCreatedMessage
        {
            UserId = user.Id,
            Email = user.Email
        });
    }
}
```

## How It Works

### Exchange and Queue Naming

- **Exchange name**: The event message type's full name (e.g., `MyApp.Events.UserCreatedMessage`). When `IncludeNamespaceForExchangeName` is `false`, only the class name is used.
- **Queue name**: `{SubscriptionName}-{ExchangeName}` — each subscriber gets its own queue bound to the exchange.
- **Routing key**: Same as the exchange name (uses direct exchange type).

### Request-Response Pattern

For `RequestMessage` / `ResponseMessage<TRequest>` types, the provider supports RPC-style communication:

1. Publisher sends a request with a `ReplyTo` header pointing to a reply queue
2. Handler processes the request and publishes the response to the reply queue
3. Publisher awaits the response with correlation ID matching

### Message Flow

```
Publisher → Exchange (direct) → Queue (per subscriber) → Consumer (hosted service) → Handler
```

## Comparison with Azure Service Bus Provider

| Feature | Azure Service Bus | RabbitMQ |
|---------|------------------|----------|
| Topic/Exchange | Topic | Direct Exchange |
| Subscription/Queue | Subscription | Queue |
| Auto-create resources | Yes | Yes |
| Request-Response | ReplyTo + CorrelationId | ReplyTo + CorrelationId |
| Duplicate detection | Built-in | Not supported |
| Message TTL | Per-entity | Per-queue |
| Max size | Per-entity | Per-queue |
