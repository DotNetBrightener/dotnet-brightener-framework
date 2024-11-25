namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native;

public abstract class ServiceBusMessageProcessor<TEventMessage>(IServiceProvider serviceProvider)
    : IServiceBusMessageProcessor
    where TEventMessage : EventMessageWrapper
{
    /// <inheritdoc />
    /// <remarks>
    ///     Must call base method
    /// </remarks>
    public virtual async Task<EventMessageWrapper> PrepareOutgoingMessage<T>(T                   message,
                                                                             EventMessageWrapper originMessage = null)
        where T : DistributedEventMessage
    {
        var eventMessage = serviceProvider.TryGet<TEventMessage>();

        eventMessage!.WithPayload(message);

        if (originMessage is not null)
        {
            eventMessage!.CorrelationId = originMessage.CorrelationId;
            eventMessage.OriginApp      = originMessage.OriginApp;
        }

        return eventMessage;
    }

    public virtual async Task<EventMessageWrapper> ParseIncomingMessage(string incomingJson)
    {
        return incomingJson.DeserializeToMessage<TEventMessage>();
    }
}