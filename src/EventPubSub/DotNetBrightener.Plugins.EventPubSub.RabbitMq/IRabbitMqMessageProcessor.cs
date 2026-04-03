namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq;

/// <summary>
///     Represents the processor to convert the message to appropriate format for RabbitMQ, and convert
///     incoming messages from RabbitMQ to the application event message format.
/// </summary>
public interface IRabbitMqMessageProcessor
{
    /// <summary>
    ///     Prepares the message payload to be sent to RabbitMQ
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the actual event message
    /// </typeparam>
    /// <param name="message">
    ///     The message payload
    /// </param>
    /// <param name="originMessage">
    ///     Specifies the original message, if the <see cref="message"/> is a result of that message
    /// </param>
    /// <returns>
    ///     The new message payload to be sent to RabbitMQ
    /// </returns>
    Task<EventMessageWrapper> PrepareOutgoingMessage<T>(T                    message,
                                                        EventMessageWrapper? originMessage = null)
        where T : IDistributedEventMessage;

    /// <summary>
    ///     Parses the incoming message from the message broker
    /// </summary>
    /// <param name="incomingJson">
    ///     The incoming message in JSON format
    /// </param>
    /// <returns>
    ///     The message payload from the given JSON
    /// </returns>
    Task<EventMessageWrapper> ParseIncomingMessage(string incomingJson);
}
