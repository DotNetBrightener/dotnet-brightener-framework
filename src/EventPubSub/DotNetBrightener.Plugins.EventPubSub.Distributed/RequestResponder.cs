// ReSharper disable CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the service that responses to a request of type <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">
///     The type of the request message.
/// </typeparam>
public abstract class RequestResponder<TRequest> : IEventHandler<TRequest>
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
}