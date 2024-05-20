using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

/// <summary>
///     The based class for any event message that is used for the Azure Service Bus.
/// </summary>
public abstract class AzureServiceBusEventMessage
{
    /// <summary>
    ///     The correlation id for the event message.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     Indicates when the event message was created.
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [JsonExtensionData]
    public Dictionary<string, object?> Payload { get; set; }

    public void WithPayload<T>(T eventMessage) where T : IDistributedEventMessage
    {
        var serializedMessage = JsonConvert.SerializeObject(eventMessage);

        Payload = JsonConvert.DeserializeObject<Dictionary<string, object?>>(serializedMessage);
    }

    public T GetPayload<T>() where T : IDistributedEventMessage
    {
        var jsonPayload = JsonConvert.SerializeObject(Payload);

        T result = JsonConvert.DeserializeObject<T>(jsonPayload);

        return result;
    }
}

internal class SimpleAzureEventMessage : AzureServiceBusEventMessage
{
}