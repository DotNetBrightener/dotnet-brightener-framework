﻿namespace DotNetBrightener.Plugins.EventPubSub;

public interface IEventHandler { }

public interface IEventHandler<T> : IEventHandler where T: IEventMessage
{
    /// <summary>
    ///		Retrieves the priority of the event handler, the higher number will be run first
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///		Process the given event message
    /// </summary>
    /// <param name="eventMessage">The event message</param>
    /// <returns>
    ///     A <see cref="bool"/> value indicates whether the event should be handled by the next handler.
    ///     <c>True</c> if the next handler should be picking the event and continue processing.
    ///     Otherwise, <c>False</c>
    /// </returns>
    Task<bool> HandleEvent(T eventMessage);
}