namespace DotNetBrightener.Plugins.EventPubSub.Distributed;

public interface IDistributedMessagePublisher
{
    Task Publish<T>(T eventMessage, EventMessageWrapper originMessage = null) where T : IDistributedEventMessage;

    Task<TResponse> GetResponse<TRequest, TResponse>(TRequest message)
        where TRequest : RequestMessage
        where TResponse : ResponseMessage<TRequest>;
}