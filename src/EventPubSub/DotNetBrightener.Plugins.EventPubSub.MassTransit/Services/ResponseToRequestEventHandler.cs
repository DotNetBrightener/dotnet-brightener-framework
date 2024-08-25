using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.MassTransit.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.MassTransit.Services;

internal class ResponseToRequestEventHandler<TEventMessage> : IConsumer<TEventMessage>
    where TEventMessage : class, IRequestMessage, new()
{
    private readonly RequestResponder<TEventMessage> _distributedEventEventResponder;
    private readonly ILogger                         _logger;

    public ResponseToRequestEventHandler(IEnumerable<IEventHandler<TEventMessage>> eventHandlers,
                                         ILoggerFactory                            loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());

        _distributedEventEventResponder = eventHandlers.OfType<RequestResponder<TEventMessage>>()
                                                       .First(); // should never be null
    }

    public async Task Consume(ConsumeContext<TEventMessage> context)
    {
        if (_distributedEventEventResponder is null)
            return;

        var eventMessage = context.Message;

        _distributedEventEventResponder.OriginPayload = new MassTransitEventMessageWrapper
        {
            CorrelationId = context.CorrelationId,
            CreatedOn     = context.SentTime,
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