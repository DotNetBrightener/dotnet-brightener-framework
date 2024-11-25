namespace DotNetBrightener.Plugins.EventPubSub;

public interface IEventHandler
{
    /// <summary>
    ///		Retrieves the priority of the event handler, the higher number will be run first
    /// </summary>
    int Priority { get; }
}

/// <summary>
///     Represents the event handler that handles the event message of the specified type
/// </summary>
/// <typeparam name="T">
///     The type of the event message
/// </typeparam>
public interface IEventHandler<in T> : IEventHandler where T : IEventMessage
{
    /// <summary>
    ///		Processes the <typeparamref name="T"/> event message
    /// </summary>
    /// <param name="eventMessage">The event message</param>
    /// <returns>
    ///     A <see cref="bool"/> value indicates whether the event should be handled by the next handler.
    ///     <c>True</c> if the next handler should be picking the event and continue processing.
    ///     Otherwise, <c>False</c>
    /// </returns>
    Task<bool> HandleEvent(T eventMessage);
}