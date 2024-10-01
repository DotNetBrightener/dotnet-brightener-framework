using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub;

public abstract class BaseEventMessage : IEventMessage
{
    /// <summary>
    ///     The correlation id for the event message.
    /// </summary>
    public Guid CorrelationId { get; set; } = Uuid7.Guid();

    /// <summary>
    ///     The unique identifier for the event message.
    /// </summary>
    public Guid EventId { get; set; } = Uuid7.Guid();

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
    public string FromApp { get; set; }

    /// <summary>
    ///     The name of the application that processes and forwards the event message, if applicable.
    /// </summary>
    public string CurrentApp { get; set; }

    /// <summary>
    ///     The name of the machine that initiates the event message.
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    [JsonExtensionData]
    public Dictionary<string, object?> Payload { get; set; }
}