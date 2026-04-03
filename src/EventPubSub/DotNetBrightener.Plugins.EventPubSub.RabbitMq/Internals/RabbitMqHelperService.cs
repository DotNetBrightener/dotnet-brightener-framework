using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal class RabbitMqHelperService(
    IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
    ILogger<RabbitMqHelperService>  logger) : IRabbitMqHelperService
{
    private readonly RabbitMqConfiguration _config = rabbitMqConfiguration.Value;
    private          IConnection           _connection;
    private readonly SemaphoreSlim         _connectionLock = new(1, 1);

    public async Task CreateExchangeIfNotExists(string exchangeName, CancellationToken cancellationToken = default)
    {
        var connection = await GetConnection(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(exchange: exchangeName,
                                           type: ExchangeType.Direct,
                                           durable: _config.DurableExchanges,
                                           autoDelete: _config.AutoDeleteExchanges,
                                           cancellationToken: cancellationToken);

        logger.LogInformation("Exchange {exchangeName} ensured to exist", exchangeName);
    }

    public async Task CreateQueueIfNotExists(string            queueName,
                                             string            exchangeName,
                                             string            routingKey,
                                             CancellationToken cancellationToken = default)
    {
        var connection = await GetConnection(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue: queueName,
                                        durable: _config.DurableQueues,
                                        exclusive: false,
                                        autoDelete: false,
                                        cancellationToken: cancellationToken);

        await channel.QueueBindAsync(queue: queueName,
                                     exchange: exchangeName,
                                     routingKey: routingKey,
                                     cancellationToken: cancellationToken);

        logger.LogInformation("Queue {queueName} ensured to exist and bound to exchange {exchangeName} with routing key {routingKey}",
                              queueName,
                              exchangeName,
                              routingKey);
    }

    public async Task CreateReceiverQueue(string queueName, CancellationToken cancellationToken = default)
    {
        var connection = await GetConnection(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue: queueName,
                                        durable: _config.DurableQueues,
                                        exclusive: false,
                                        autoDelete: false,
                                        cancellationToken: cancellationToken);

        logger.LogInformation("Receiver queue {queueName} ensured to exist", queueName);
    }

    public async Task<IConnection> GetConnection(CancellationToken cancellationToken = default)
    {
        if (_connection is not null &&
            _connection.IsOpen)
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is not null &&
                _connection.IsOpen)
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName    = _config.HostName,
                Port        = _config.Port,
                VirtualHost = _config.VirtualHost,
                UserName    = _config.UserName,
                Password    = _config.Password,
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            logger.LogInformation("Connected to RabbitMQ at {hostName}:{port}{virtualHost}",
                                  _config.HostName,
                                  _config.Port,
                                  _config.VirtualHost);

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}