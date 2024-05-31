namespace DotNetBrightener.Plugins.EventPubSub;

public abstract class DistributedEventEventHandler<T> : IEventHandler<T> where T : IDistributedEventMessage
{
    public int Priority => 1000;

    public EventMessageWrapper OriginPayload { get; internal set; }

    public abstract Task<bool> HandleEvent(T eventMessage);
}