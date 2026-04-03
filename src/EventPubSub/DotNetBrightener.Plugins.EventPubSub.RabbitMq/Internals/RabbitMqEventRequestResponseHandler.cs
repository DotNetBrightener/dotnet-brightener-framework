using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal interface IRabbitMqEventRequestResponseHandler
{
    Task<bool> ProcessMessage(EventMessageWrapper messageWrapper,
                              IChannel            channel,
                              ulong               deliveryTag,
                              BasicProperties     basicProperties);
}

internal class RabbitMqEventRequestResponseHandler<T>(
    IEnumerable<IEventHandler<T>> eventHandlers,
    ILoggerFactory                loggerFactory,
    IRabbitMqMessageProcessor     messageProcessor)
    : IRabbitMqEventRequestResponseHandler
    where T : RequestMessage, new()
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RabbitMqEventRequestResponseHandler<T>>();

    public async Task<bool> ProcessMessage(EventMessageWrapper messageWrapper,
                                           IChannel            channel,
                                           ulong               deliveryTag,
                                           BasicProperties     basicProperties)
    {
        if (!eventHandlers.Any())
            return false;

        var eventMessage = ExtractPayload<T>(messageWrapper);

        if (eventMessage == null)
        {
            _logger.LogInformation("No message body to process for request type {requestType}.",
                                   typeof(T).GetExchangeName());

            return false;
        }

        if (string.IsNullOrWhiteSpace(eventMessage.FromApp))
            eventMessage.FromApp = messageWrapper.CurrentApp;

        if (string.IsNullOrWhiteSpace(eventMessage.OriginApp))
            eventMessage.OriginApp = messageWrapper.OriginApp;

        var orderedEventHandlers = eventHandlers.OrderByDescending(eventHandler => eventHandler.Priority)
                                                .ToArray();

        IResponseMessage<T> responseMessage = null;

        foreach (var handler in orderedEventHandlers)
        {
            // set origin payload via reflection since OriginPayload may be read-only
            var originProp = handler.GetType().GetProperty("OriginPayload");
            originProp?.SetValue(handler, messageWrapper);

            _logger.LogInformation("Processing request {requestType} using handler {handlerName}",
                                   eventMessage.GetType().GetExchangeName(),
                                   handler.GetType().Name);

            await handler.HandleEvent(eventMessage);

            // capture response from handler implementing IResponseMessage<T>
            if (handler is IResponseMessage<T> responder)
                responseMessage = responder;
        }

        // send response back if reply-to is specified
        if (!string.IsNullOrEmpty(basicProperties.ReplyTo) &&
            responseMessage is not null)
        {
            await SendResponseToSender(channel, basicProperties, responseMessage, messageWrapper);
        }

        return true;
    }

    private async Task SendResponseToSender(IChannel            channel,
                                            BasicProperties     requestProperties,
                                            IResponseMessage<T> message,
                                            EventMessageWrapper originMessage)
    {
        var responseWrapper = await messageProcessor.PrepareOutgoingMessage((IDistributedEventMessage)message,
                                                                            originMessage);

        var json = JsonConvert.SerializeObject(responseWrapper);
        var body = Encoding.UTF8.GetBytes(json);

        var replyProperties = new BasicProperties
        {
            CorrelationId = requestProperties.CorrelationId,
        };

        await channel.BasicPublishAsync(exchange: string.Empty,
                                        routingKey: requestProperties.ReplyTo,
                                        mandatory: false,
                                        basicProperties: replyProperties,
                                        body: new ReadOnlyMemory<byte>(body));

        _logger.LogInformation("Response sent to reply queue {replyTo} with correlation {correlationId}",
                               requestProperties.ReplyTo,
                               requestProperties.CorrelationId);
    }

    /// <summary>
    ///     Extracts the typed payload from the message wrapper using JSON deserialization.
    ///     Replaces the unavailable GetPayload&lt;T&gt;() extension method.
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
