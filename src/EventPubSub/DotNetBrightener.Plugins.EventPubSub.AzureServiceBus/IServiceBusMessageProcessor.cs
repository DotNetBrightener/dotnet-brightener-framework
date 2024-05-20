using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

public interface IServiceBusMessageProcessor
{
    Task<AzureServiceBusEventMessage> PreprocessMessage<T>(T message) where T : IDistributedEventMessage;

    Task<AzureServiceBusEventMessage> ProcessIncomingMessage(string incomingJson);
}

public class ServiceBusMessageProcessor<TEventMessage> : IServiceBusMessageProcessor
    where TEventMessage : AzureServiceBusEventMessage
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceBusMessageProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<AzureServiceBusEventMessage> PreprocessMessage<T>(T message) where T : IDistributedEventMessage
    {
        var eventMessage = _serviceProvider.TryGet<TEventMessage>();

        eventMessage.WithPayload(message);

        return eventMessage;
    }

    public async Task<AzureServiceBusEventMessage> ProcessIncomingMessage(string incomingJson)
    {
        var eventMessage = JsonConvert.DeserializeObject<TEventMessage>(incomingJson);

        return eventMessage;
    }
}