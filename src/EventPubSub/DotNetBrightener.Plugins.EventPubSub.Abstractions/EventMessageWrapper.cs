#nullable enable
using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     The wrapper class for event messages that is used to contain the message
///     The actual event message is stored in the <see cref="Payload"/> property.
/// </summary>
public abstract class EventMessageWrapper : BaseEventMessage
{
    /// <summary>
    ///     The actual event message
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> Payload { get; internal set; }
}