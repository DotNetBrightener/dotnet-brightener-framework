namespace DotNetBrightener.Plugins.EventPubSub;

/// <summary>
///     Represents the message used to request a response from the event handler.
///     Suitable in distributed systems using request-response pattern.
/// </summary>
public interface IRequestMessage : IEventMessage;

/// <summary>
///     Marks a class as a response message.
/// </summary>
public interface IResponseMessage : IEventMessage;


/// <summary>
///     Represents the message used to response to the <see cref="IRequestMessage"/> from the event handler.
///     Suitable in distributed systems using request-response pattern.
/// </summary>
public interface IResponseMessage<TRequest> : IResponseMessage where TRequest : class, IRequestMessage;
