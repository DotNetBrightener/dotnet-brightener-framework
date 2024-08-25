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
///     The distributed event message data should not be modified by the event handlers.
/// </remarks>
public abstract class DistributedEventMessage : IEventMessage
{
    /// <summary>
    ///     The correlation id for the event message.
    /// </summary>
    public Guid CorrelationId { get; set; } = Ulid.NewUlid().ToGuid();

    /// <summary>
    ///     The unique identifier for the event message.
    /// </summary>
    public Guid EventId { get; set; } = Ulid.NewUlid().ToGuid();

    /// <summary>
    ///     Indicates when the event message was created.
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}