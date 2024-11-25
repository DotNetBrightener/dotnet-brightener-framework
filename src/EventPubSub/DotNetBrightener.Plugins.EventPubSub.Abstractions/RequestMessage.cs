namespace DotNetBrightener.Plugins.EventPubSub;

public interface IRequestMessage : IDistributedEventMessage;

public interface IResponseMessage : IDistributedEventMessage;

public interface IResponseMessage<TRequest> : IResponseMessage where TRequest : RequestMessage;

/// <summary>
///     Represents the message used to request a response from the event handler.
///     Suitable in distributed systems using request-response pattern.
/// </summary>
public abstract class RequestMessage : BaseEventMessage, IRequestMessage;

/// <summary>
///     Marks a class as a response message.
/// </summary>
public abstract class ResponseMessage : BaseEventMessage, IResponseMessage;


/// <summary>
///     Represents the message used to response to the <see cref="IRequestMessage"/> from the event handler.
///     Suitable in distributed systems using request-response pattern.
/// </summary>
public abstract class ResponseMessage<TRequest> : ResponseMessage, IResponseMessage<TRequest> where TRequest : RequestMessage;
