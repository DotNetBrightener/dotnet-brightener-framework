// /**
// * DotNetBrightener - DotNetBrightener.Framework
// * Copyright (c) 2021 DotNetBrightener, LLC.
// */

namespace DotNetBrightener.Plugins.EventPubSub
{
    /// <summary>
    ///     Represents the event message used in <see cref="IEventHandler{T}"/>,
    ///     but all of its <see cref="IEventHandler{T}"/>s will all be executed,
    ///     even if the implemented <see cref="IEventHandler{T}.HandleEvent"/> method returns <c>false</c>
    /// </summary>
    public interface INonStoppedEventMessage : IEventMessage
    {
        
    }
}