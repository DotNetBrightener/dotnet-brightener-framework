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
}