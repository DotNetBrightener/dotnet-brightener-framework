// ReSharper disable CheckNamespace

using DotNetBrightener.Plugins.EventPubSub.Distributed.Services;

namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the Event Handler service that processes the distributed event message of type <typeparamref name="TEventMessage" /> in a distributed environment.
/// </summary>
/// <typeparam name="TEventMessage">
///     The type of the distributed event message.
/// </typeparam>
public abstract class DistributedEventHandler<TEventMessage> : IEventHandler<TEventMessage>
    where TEventMessage : DistributedEventMessage
{
    public int Priority => 1000;

    public EventMessageWrapper OriginPayload { get; internal set; }

    public abstract Task<bool> HandleEvent(TEventMessage eventMessage);

    async Task IConsumer<TEventMessage>.Consume(ConsumeContext<TEventMessage> context)
    {
        var eventMessage = context.Message;

        OriginPayload = new DistributedEventMessageWrapper
        {
            CorrelationId = eventMessage.CorrelationId,
            CreatedOn     = eventMessage.CreatedOn,
            MachineName   = eventMessage.MachineName ?? context.SourceAddress?.Host,
            OriginApp     = eventMessage.OriginApp,
            EventId       = eventMessage.EventId,
            Payload       = eventMessage.Payload
        };


        await HandleEvent(eventMessage);
    }
}