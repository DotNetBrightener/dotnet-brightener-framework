namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the event message used in <see cref="IEventHandler{T}"/>
/// </summary>
/// <remarks>
///     The event message can be modified by the event handlers if the event is published without `running in background` enabled
/// </remarks>
public interface IEventMessage;

/// <summary>
///     Represents the event message that can be published to distributed systems, such as Azure Service Bus, RabbitMQ,
///     and consumed by the distributed event subscriptions.
/// </summary>
/// <remarks>
///     The distributed event message data will not be modified by the event handlers.
/// </remarks>
public interface IDistributedEventMessage : IEventMessage;