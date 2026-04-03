using RabbitMQ.Client;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal interface IRabbitMqHelperService
{
    /// <summary>
    ///     Creates an exchange if it does not already exist
    /// </summary>
    Task CreateExchangeIfNotExists(string exchangeName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a queue bound to the specified exchange if it does not already exist
    /// </summary>
    Task CreateQueueIfNotExists(string            queueName,
                                string            exchangeName,
                                string            routingKey,
                                CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a receiver queue for RPC response messages
    /// </summary>
    Task CreateReceiverQueue(string queueName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets or creates a shared RabbitMQ connection
    /// </summary>
    Task<IConnection> GetConnection(CancellationToken cancellationToken = default);
}
