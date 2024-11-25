namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the event message used in <see cref="IEventHandler{T}"/>
/// </summary>
/// <remarks>
///     The event message can be modified by the event handlers if the event is published without `running in background` enabled
/// </remarks>
public interface IEventMessage;

/// <summary>
///     Represents the event message that could be used in both local and distributed event handlers
/// </summary>
public interface ICombinationEventMessage : IEventMessage;

public interface IDistributedEventMessage: IEventMessage
{
    /// <summary>
    ///     The correlation id for the event message.
    /// </summary>
    Guid CorrelationId { get; set; }

    /// <summary>
    ///     The unique identifier for the event message.
    /// </summary>
    Guid EventId { get; set; }

    /// <summary>
    ///     Indicates when the event message was created.
    /// </summary>
    DateTime CreatedOn { get; set; }

    /// <summary>
    ///     The name of the application that initiates the event message.
    /// </summary>
    string OriginApp { get; set; }

    /// <summary>
    ///     The name of the application that processes and forwards the event message, if applicable.
    /// </summary>
    string FromApp { get; set; }

    /// <summary>
    ///     The name of the application that processes and forwards the event message, if applicable.
    /// </summary>
    string CurrentApp { get; set; }

    /// <summary>
    ///     The name of the machine that initiates the event message.
    /// </summary>
    string MachineName { get; set; }
}