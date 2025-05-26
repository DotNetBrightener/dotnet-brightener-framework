using Azure.Messaging.ServiceBus;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Extensions;
using Microsoft.Extensions.Options;
using System.Text;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal interface IServiceBusMessagePublisher
{
    Task Publish<T>(T eventMessage, EventMessageWrapper originMessage = null) where T : DistributedEventMessage;
}

internal class ServiceBusMessagePublisher(
    IOptions<ServiceBusConfiguration> serviceBusConfiguration,
    IServiceBusMessageProcessor messageProcessor) : IServiceBusMessagePublisher
{
    public async Task Publish<T>(T eventMessage, EventMessageWrapper originMessage = null)
        where T : DistributedEventMessage
    {
        var messageToSend = await messageProcessor.PrepareOutgoingMessage(eventMessage, originMessage);

        if (string.IsNullOrEmpty(messageToSend.OriginApp))
        {
            // only set the origin app if it is not already set
            messageToSend.OriginApp = serviceBusConfiguration.Value.SubscriptionName;
        }

        messageToSend.CurrentApp = serviceBusConfiguration.Value.SubscriptionName;

        var json = messageToSend.ToJson();

        var msgBody = new ServiceBusMessage(Encoding.UTF8.GetBytes(json));

        var topicName = eventMessage.GetType().GetTopicName();

        await using var client = new ServiceBusClient(serviceBusConfiguration.Value.ConnectionString);

        await using var sender = client.CreateSender(topicName);

        await sender.SendMessageAsync(msgBody);
    }
}