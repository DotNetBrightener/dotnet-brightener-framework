using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.Distributed;
using DotNetBrightener.Plugins.EventPubSub.Distributed.Services;
using MassTransit;
// ReSharper disable CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the service that responses to a request of type <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">
///     The type of the request message.
/// </typeparam>
public abstract class RequestResponder<TRequest> : IEventHandler<TRequest>, IConsumer<TRequest>
    where TRequest : RequestMessage, new()
{
    public int Priority => 1000;

    public EventMessageWrapper OriginPayload { get; internal set; }

    public abstract Task<bool> HandleEvent(TRequest eventMessage);

    internal Func<IResponseMessage<TRequest>, Task> SendResponseInternal { get; set; }

    /// <summary>
    ///     Sends the specified response to the request.
    /// </summary>
    protected Func<IResponseMessage<TRequest>, Task> SendResponse => SendResponseInternal;

    async Task IConsumer<TRequest>.Consume(ConsumeContext<TRequest> context)
    {
        var eventMessage   = context.Message;
        var currentAppName = context.GetServiceOrCreateInstance<IDistributedEventPubSubConfigurator>()?.AppName;

        OriginPayload = new DistributedEventMessageWrapper
        {
            CorrelationId = eventMessage.CorrelationId,
            CreatedOn     = eventMessage.CreatedOn,
            MachineName   = eventMessage.MachineName ?? context.SourceAddress?.Host,
            OriginApp     = eventMessage.OriginApp,
            EventId       = eventMessage.EventId,
            Payload       = eventMessage.Payload
        };

        SendResponseInternal = async responseMessage =>
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
                responseMessage.CurrentApp    = currentAppName;
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

        await HandleEvent(eventMessage);
    }
}