using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

internal interface IServiceBusMessagePublisher
{
    Task SendMessageAsync<T>(T eventMessage) where T : IDistributedEventMessage;
}

internal class ServiceBusMessagePublisher(
    IOptions<ServiceBusConfiguration> serviceBusConfiguration,
    IServiceBusMessageProcessor       messageProcessor) : IServiceBusMessagePublisher
{
    private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public async Task SendMessageAsync<T>(T eventMessage) where T : IDistributedEventMessage
    {
        var messageToSend = await messageProcessor.PreprocessMessage(eventMessage);

        var json = JsonConvert.SerializeObject(messageToSend, DefaultSerializerSettings);

        var msgBody = new ServiceBusMessage(Encoding.UTF8.GetBytes(json));

        var topicName = eventMessage.GetType().GetTopicName();

        await using (var client = new ServiceBusClient(serviceBusConfiguration.Value.ConnectionString))
        {
            ServiceBusSender sender = client.CreateSender(topicName);

            await sender.SendMessageAsync(msgBody);
        }
    }
}