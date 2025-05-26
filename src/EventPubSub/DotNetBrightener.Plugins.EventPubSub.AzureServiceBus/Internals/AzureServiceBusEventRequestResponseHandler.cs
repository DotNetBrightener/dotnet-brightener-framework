using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal interface IAzureServiceBusEventRequestResponseHandler
{
    Task<bool> ProcessMessage(EventMessageWrapper     messageWrapper,
                              ProcessMessageEventArgs args,
                              ServiceBusClient        serviceBusClient);
}

internal class AzureServiceBusEventRequestResponseHandler<T>(
    IEnumerable<IEventHandler<T>> eventHandlers,
    ILoggerFactory                loggerFactory,
    IServiceBusMessageProcessor   messageProcessor)
    : IAzureServiceBusEventRequestResponseHandler
    where T : RequestMessage, new()
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AzureServiceBusEventRequestResponseHandler<T>>();

    public async Task<bool> ProcessMessage(EventMessageWrapper     messageWrapper,
                                           ProcessMessageEventArgs args,
                                           ServiceBusClient        serviceBusClient)
    {
        if (!eventHandlers.Any())
        {
            return false;
        }

        var eventMessage = messageWrapper.GetPayload<T>();

        if (eventMessage == null)
        {

            _logger.LogInformation("No message body to process for topic {topicName}.",
                                   typeof(T).GetTopicName());

            return false;
        }

        if (string.IsNullOrWhiteSpace(eventMessage.FromApp))
        {
            eventMessage.FromApp = messageWrapper.CurrentApp;
        }

        if (string.IsNullOrWhiteSpace(eventMessage.OriginApp))
        {
            eventMessage.OriginApp = messageWrapper.OriginApp;
        }

        var orderedEventHandlers = eventHandlers.OrderByDescending(eventHandler => eventHandler.Priority)
                                                .ToArray();

        foreach (var handler in orderedEventHandlers)
        {
            if (handler is RequestResponder<T> distributedEventHandler)
            {
                distributedEventHandler.OriginPayload = messageWrapper;
                distributedEventHandler.SendResponseInternal = async message =>
                {
                    await SendResponseToSender(args, serviceBusClient, message, messageWrapper);
                };
            }

            _logger.LogInformation("Processing message {topicName} using handler {handlerName}",
                                   eventMessage.GetType().GetTopicName(),
                                   handler.GetType().Name);

            await handler.HandleEvent(eventMessage);
        }

        return true;
    }

    private async Task SendResponseToSender(ProcessMessageEventArgs args,
                                            ServiceBusClient        serviceBusClient,
                                            IResponseMessage<T>     message,
                                            EventMessageWrapper     originMessage)
    {
        var messageToSend = await messageProcessor.PrepareOutgoingMessage(message, originMessage);
        messageToSend.FromApp = originMessage?.CurrentApp;
        var json = messageToSend.ToJson();

        var msgBody = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
        {
            CorrelationId = originMessage.CorrelationId.ToString(),
            SessionId     = originMessage.CorrelationId.ToString(),
        };

        await using var sender = serviceBusClient.CreateSender(args.Message.ReplyTo);

        await sender.SendMessageAsync(msgBody);
    }
}