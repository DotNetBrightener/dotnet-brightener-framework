using System.Threading.Tasks;

namespace DotNetBrightener.Plugins.EventPubSub;

public interface IEventPublisher
{
    /// <summary>
    ///     Fires an event of type <typeparamref name="T"/>, optionally specify if it should be executed in background
    /// </summary>
    /// <typeparam name="T">Type of the event message</typeparam>
    /// <param name="eventMessage">The event message</param>
    /// <param name="runInBackground">
    ///     If <c>true</c>, another thread will be used to execute the task.
    ///     The thread will not share the resources objects from the calling scope so be cautioned using this property as <c>true</c>
    /// </param>
    Task Publish<T>(T eventMessage, bool runInBackground = false) where T : class, IEventMessage;
}