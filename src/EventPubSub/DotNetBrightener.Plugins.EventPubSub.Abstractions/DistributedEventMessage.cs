namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the event message that can be published to distributed systems, such as Azure Service Bus, RabbitMQ,
///     and consumed by the distributed event subscriptions.
/// </summary>
/// <remarks>
///     The distributed event message data should not be modified by the event handlers.
/// </remarks>
public abstract class DistributedEventMessage : BaseEventMessage, IDistributedEventMessage;