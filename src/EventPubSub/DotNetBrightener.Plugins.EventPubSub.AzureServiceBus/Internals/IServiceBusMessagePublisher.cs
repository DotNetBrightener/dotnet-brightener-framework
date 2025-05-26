using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using DotNetBrightener.Plugins.EventPubSub.Distributed;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal class DistributedMessagePublisher(
    IOptions<ServiceBusConfiguration> serviceBusConfiguration,
    IServiceBusMessageProcessor       messageProcessor) : IDistributedMessagePublisher
{
    public async Task Publish<T>(T eventMessage, EventMessageWrapper originMessage = null)
        where T : IDistributedEventMessage
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

    public async Task<TResponse> GetResponse<TRequest, TResponse>(TRequest message)
        where TRequest : RequestMessage
        where TResponse : ResponseMessage<TRequest>
    {
        var messageToSend = await messageProcessor.PrepareOutgoingMessage(message);

        var subscriptionName = serviceBusConfiguration.Value.SubscriptionName;

        if (string.IsNullOrEmpty(messageToSend.OriginApp))
        {
            // only set the origin app if it is not already set
            messageToSend.OriginApp = subscriptionName;
        }

        messageToSend.CurrentApp = subscriptionName;

        var json = messageToSend.ToJson();


        var topicName = message.GetType().GetTopicName();

        await using var client = new ServiceBusClient(serviceBusConfiguration.Value.ConnectionString);

        await using var sender = client.CreateSender(topicName);

        var replyPath = $"receiver-{messageToSend.CurrentApp}-{typeof(TResponse).GetTopicName()}";

        var correlationId = messageToSend.CorrelationId.ToString();
        await using var receiver      = client.CreateReceiver(replyPath);

        var msgBody = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
        {
            ReplyTo       = replyPath,
            CorrelationId = correlationId,
            SessionId     = correlationId,
        };

        await sender.SendMessageAsync(msgBody);

        var deadline = DateTime.UtcNow.Add(serviceBusConfiguration.Value.ResponseTimeout);

        while (true)
        {
            if (deadline < DateTime.UtcNow)
                break;

            var responseMessage = await receiver.ReceiveMessageAsync();

            if (responseMessage != null &&
                responseMessage.CorrelationId == message.CorrelationId.ToString())
            {
                string responseBody = responseMessage.Body.ToString();
                await receiver.CompleteMessageAsync(responseMessage);

                var response = responseBody.DeserializeToMessage<SimpleAzureEventMessageWrapper>();

                var responsePayload = response.GetPayload<TResponse>();
                responsePayload.FromApp    = response.FromApp;
                responsePayload.CurrentApp = subscriptionName;
                responsePayload.OriginApp  = response.OriginApp;


                return responsePayload;
            }
        }

        throw new TimeoutException("Response not received within the expected timeframe.");
    }
}