using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.Distributed.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed.Services;

internal class ResponseToRequestEventHandler<TEventMessage> : IConsumer<TEventMessage>
    where TEventMessage : RequestMessage, new()
{
    private readonly RequestResponder<TEventMessage>     _distributedEventEventResponder;
    private readonly IDistributedEventPubSubConfigurator _configurator;
    private readonly ILogger                             _logger;

    public ResponseToRequestEventHandler(IEnumerable<IEventHandler<TEventMessage>> eventHandlers,
                                         ILoggerFactory                            loggerFactory,
                                         IDistributedEventPubSubConfigurator       configurator)
    {
        _configurator = configurator;
        _logger       = loggerFactory.CreateLogger(GetType());

        _distributedEventEventResponder = eventHandlers.OfType<RequestResponder<TEventMessage>>()
                                                       .First(); // should never be null
    }

    public async Task Consume(ConsumeContext<TEventMessage> context)
    {
        if (_distributedEventEventResponder is null)
            return;

        var eventMessage = context.Message;

        _distributedEventEventResponder.OriginPayload = new DistributedEventMessageWrapper
        {
            CorrelationId = context.CorrelationId ?? Guid.Empty,
            CreatedOn     = context.SentTime ?? DateTime.UtcNow,
            MachineName   = context.SourceAddress?.ToString()
        };

        var serializedMessage = JsonConvert.SerializeObject(eventMessage, JsonConfig.SerializeOptions);

        _distributedEventEventResponder.OriginPayload
                                       .Payload =
            JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedMessage,
                                                                      JsonConfig.DeserializeOptions);

        _logger.LogInformation("Processing messages {correlationId} sent from {source} at {createdOn}",
                               context.CorrelationId,
                               context.SentTime,
                               context.SourceAddress);

        _distributedEventEventResponder.SendResponseInternal = async responseMessage =>
        {
            MethodInfo responseAsyncMethod = context.GetType()
                                                    .GetMethods()
                                                    .FirstOrDefault(m => m.Name == nameof(context.RespondAsync) &&
                                                                         m.IsGenericMethod &&
                                                                         m.GetGenericArguments().Length == 1 &&
                                                                         m.GetParameters().Length == 1);

            if (responseAsyncMethod is not null)
            {
                var invokingMethod = responseAsyncMethod.MakeGenericMethod(responseMessage.GetType());

                responseMessage.CorrelationId = eventMessage.CorrelationId;
                responseMessage.OriginApp     = eventMessage.OriginApp;
                responseMessage.FromApp       = eventMessage.CurrentApp;
                responseMessage.CurrentApp    = _configurator.AppName;
                responseMessage.CreatedOn     = DateTime.UtcNow;

                if (invokingMethod.Invoke(context,
                    [
                        responseMessage
                    ]) is Task task)
                {
                    await task;
                }
            }
        };

        await _distributedEventEventResponder.HandleEvent(eventMessage);
    }
}