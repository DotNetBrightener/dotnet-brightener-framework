namespace DotNetBrightener.Plugins.EventPubSub;

public interface IEventPublisher
{
    /// <summary>
    ///     Fires an event of type <typeparamref name="T"/>, optionally specify if it should be executed in background
    /// </summary>
    /// <typeparam name="T">
    ///     Type of the event message
    /// </typeparam>
    /// <param name="eventMessage">
    ///     The event message
    /// </param>
    /// <param name="originMessage">
    ///     The wrapper of original event message that initiates the <see cref="eventMessage"/>
    /// </param>
    /// <param name="runInBackground">
    ///     If <c>true</c>, another thread will be used to execute the task.
    ///     The thread will not share the resources objects from the calling scope so be cautioned using this property as <c>true</c>
    /// </param>
    Task Publish<T>(T eventMessage, bool runInBackground = false, EventMessageWrapper originMessage = null)
        where T : class, IEventMessage;


    Task<TResponse> GetResponse<TResponse, TRequest>(TRequest requestMessage)
        where TRequest : class, IRequestMessage
        where TResponse : class, IResponseMessage<TRequest>;

    Task<(TResponse, TErrorResponse)> GetResponse<TResponse, TErrorResponse, TRequest>(TRequest requestMessage)
        where TResponse : class, IResponseMessage<TRequest>
        where TErrorResponse : class, IResponseMessage<TRequest>
        where TRequest : class, IRequestMessage;
}