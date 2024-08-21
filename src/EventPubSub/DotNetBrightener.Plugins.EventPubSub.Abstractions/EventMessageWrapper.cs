using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     The wrapper class for event messages that is used to contain the message
///     The actual event message is stored in the <see cref="Payload"/> property.
/// </summary>
public abstract class EventMessageWrapper
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

    /// <summary>
    ///     The name of the application that initiates the event message.
    /// </summary>
    public string OriginApp { get; set; }

    /// <summary>
    ///     The name of the application that processes and forwards the event message, if applicable.
    /// </summary>
    public string CurrentApp { get; set; }

    /// <summary>
    ///     The name of the machine that initiates the event message.
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    ///     The actual event message
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> Payload { get; internal set; }
}