using DotNetBrightener.Plugins.EventPubSub.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal class DistributedMessagePublisher(
    IOptions<RabbitMqConfiguration>      rabbitMqConfiguration,
    IRabbitMqHelperService               helperService,
    IRabbitMqMessageProcessor            messageProcessor,
    ILogger<DistributedMessagePublisher> logger) : IDistributedMessagePublisher
{
    private readonly RabbitMqConfiguration _config = rabbitMqConfiguration.Value;

    public async Task Publish<T>(T eventMessage, EventMessageWrapper originMessage = null)
        where T : IDistributedEventMessage
    {
        var messageToSend = await messageProcessor.PrepareOutgoingMessage(eventMessage, originMessage);

        if (string.IsNullOrEmpty(messageToSend.OriginApp))
            messageToSend.OriginApp = _config.SubscriptionName;

        messageToSend.CurrentApp = _config.SubscriptionName;

        var json = JsonConvert.SerializeObject(messageToSend);
        var body = Encoding.UTF8.GetBytes(json);

        var exchangeName = eventMessage.GetType().GetExchangeName();
        var routingKey   = exchangeName;

        var             connection = await helperService.GetConnection();
        await using var channel    = await connection.CreateChannelAsync();

        await channel.BasicPublishAsync(exchange: exchangeName,
                                        routingKey: routingKey,
                                        mandatory: false,
                                        body: new ReadOnlyMemory<byte>(body));

        logger.LogInformation("Published message to exchange {exchangeName}", exchangeName);
    }

    public async Task<TResponse> GetResponse<TRequest, TResponse>(TRequest message)
        where TRequest : RequestMessage
        where TResponse : ResponseMessage<TRequest>
    {
        var messageToSend = await messageProcessor.PrepareOutgoingMessage(message);

        var subscriptionName = _config.SubscriptionName;

        if (string.IsNullOrEmpty(messageToSend.OriginApp))
            messageToSend.OriginApp = subscriptionName;

        messageToSend.CurrentApp = subscriptionName;

        var json = JsonConvert.SerializeObject(messageToSend);
        var body = Encoding.UTF8.GetBytes(json);

        var exchangeName = message.GetType().GetExchangeName();
        var routingKey   = exchangeName;

        var replyQueueName = $"receiver-{messageToSend.CurrentApp}-{typeof(TResponse).GetExchangeName()}";
        var correlationId  = messageToSend.CorrelationId.ToString();

        var             connection = await helperService.GetConnection();
        await using var channel    = await connection.CreateChannelAsync();

        // ensure reply queue exists
        await helperService.CreateReceiverQueue(replyQueueName);

        var properties = new BasicProperties
        {
            ReplyTo       = replyQueueName,
            CorrelationId = correlationId,
        };

        await channel.BasicPublishAsync(exchange: exchangeName,
                                        routingKey: routingKey,
                                        mandatory: false,
                                        basicProperties: properties,
                                        body: new ReadOnlyMemory<byte>(body));

        // wait for response on the reply queue
        // In RabbitMQ.Client v7, we use a simple polling approach for response
        using var cts = new CancellationTokenSource(_config.ResponseTimeout);

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var result = await channel.BasicGetAsync(replyQueueName, autoAck: true, cts.Token);

                if (result is not null &&
                    result.BasicProperties.CorrelationId == correlationId)
                {
                    var responseJson = Encoding.UTF8.GetString(result.Body.Span);
                    var response     = JsonConvert.DeserializeObject<SimpleRabbitMqEventMessageWrapper>(responseJson);

                    var responsePayload = ExtractPayload<TResponse>(response);

                    if (responsePayload is not null)
                    {
                        responsePayload.FromApp    = response.FromApp;
                        responsePayload.CurrentApp = subscriptionName;
                        responsePayload.OriginApp  = response.OriginApp;

                        return responsePayload;
                    }
                }

                await Task.Delay(100, cts.Token);
            }

            throw new TimeoutException("Response not received within the expected timeframe.");
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Response not received within the expected timeframe.");
        }
    }

    /// <summary>
    ///     Extracts the typed payload from the message wrapper using JSON deserialization.
    /// </summary>
    private static TPayload ExtractPayload<TPayload>(EventMessageWrapper wrapper) where TPayload : class
    {
        var typeName = typeof(TPayload).FullName;

        if (wrapper.Payload is not null &&
            wrapper.Payload.TryGetValue(typeName, out var payloadObj) &&
            payloadObj is string payloadJson)
        {
            return JsonConvert.DeserializeObject<TPayload>(payloadJson);
        }

        return null;
    }
}
