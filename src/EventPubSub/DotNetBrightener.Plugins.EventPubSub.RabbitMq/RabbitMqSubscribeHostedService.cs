using System.Text;
using System.Collections.Concurrent;
using DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq;

internal class RabbitMqSubscribeHostedService(
    ILogger<RabbitMqSubscribeHostedService> logger,
    IOptions<RabbitMqConfiguration>         rabbitMqConfiguration,
    RabbitMqHandlerMapping                  eventBusHandlerTypesMapping,
    IServiceScopeFactory                    serviceScopeFactory,
    ILoggerFactory                          loggerFactory) : IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<Type, IChannel> _consumerChannels = new();
    private readonly string _subscriptionName = rabbitMqConfiguration.Value.SubscriptionName;
    private          IConnection _connection;
    private          IServiceScope _serviceScope;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("RabbitMQ Background Subscription Service is starting...");

        _serviceScope = serviceScopeFactory.CreateScope();
        var helperService = _serviceScope.ServiceProvider.GetRequiredService<IRabbitMqHelperService>();

        _connection = await helperService.GetConnection(cancellationToken);

        var consumerTasks = eventBusHandlerTypesMapping.Select(pair =>
                                                                   CreateConsumer(pair.Key,
                                                                                  pair.Value,
                                                                                  helperService,
                                                                                  cancellationToken));

        await Task.WhenAll(consumerTasks);
    }

    private async Task CreateConsumer(Type                   messageType,
                                      Type                   handlerType,
                                      IRabbitMqHelperService helperService,
                                      CancellationToken      cancellationToken)
    {
        var exchangeName = messageType.GetExchangeName();
        var queueName    = $"{_subscriptionName}-{exchangeName}";
        var routingKey   = exchangeName;

        await helperService.CreateExchangeIfNotExists(exchangeName, cancellationToken);
        await helperService.CreateQueueIfNotExists(queueName, exchangeName, routingKey, cancellationToken);

        // for response types, also create the receiver queue
        if (messageType.IsAssignableTo(typeof(IResponseMessage)))
        {
            var receiverQueueName = $"receiver-{_subscriptionName}-{exchangeName}";
            await helperService.CreateReceiverQueue(receiverQueueName, cancellationToken);
        }

        if (handlerType is null)
            return;

        var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            await ProcessMessage(ea, handlerType, channel);
        };

        await channel.BasicConsumeAsync(queue: queueName,
                                        autoAck: false,
                                        consumer: consumer,
                                        cancellationToken: cancellationToken);

        _consumerChannels.TryAdd(messageType, channel);

        logger.LogInformation("Subscription {subscriptionName} for exchange {exchangeName} is listening",
                              _subscriptionName,
                              exchangeName);
    }

    private async Task ProcessMessage(BasicDeliverEventArgs args, Type handlerType, IChannel channel)
    {
        var bytesAsString = Encoding.UTF8.GetString(args.Body.Span);

        using var scope = serviceScopeFactory.CreateScope();

        var messageProcessor = scope.ServiceProvider.GetRequiredService<IRabbitMqMessageProcessor>();

        var domainEvent = await messageProcessor.ParseIncomingMessage(bytesAsString);

        domainEvent.CurrentApp = _subscriptionName;

        var handlerInstance = scope.ServiceProvider.GetService(handlerType);

        if (handlerInstance is IRabbitMqEventSubscription handler)
        {
            var shouldAck = await handler.ProcessMessage(domainEvent);

            if (shouldAck)
                await channel.BasicAckAsync(args.DeliveryTag, false);
            else
                await channel.BasicNackAsync(args.DeliveryTag, false, true);
        }
        else if (handlerInstance is IRabbitMqEventRequestResponseHandler responder)
        {
            var basicProperties = new BasicProperties
            {
                CorrelationId   = args.BasicProperties.CorrelationId,
                ReplyTo         = args.BasicProperties.ReplyTo,
                ContentType     = args.BasicProperties.ContentType,
                ContentEncoding = args.BasicProperties.ContentEncoding,
                DeliveryMode    = args.BasicProperties.DeliveryMode,
                Priority        = args.BasicProperties.Priority,
                Expiration      = args.BasicProperties.Expiration,
                UserId          = args.BasicProperties.UserId,
                AppId           = args.BasicProperties.AppId,
            };

            var shouldAck = await responder.ProcessMessage(domainEvent, channel, args.DeliveryTag, basicProperties);

            if (shouldAck)
                await channel.BasicAckAsync(args.DeliveryTag, false);
            else
                await channel.BasicNackAsync(args.DeliveryTag, false, true);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Signalling RabbitMQ Consumers to stop...");

        foreach (var (_, channel) in _consumerChannels)
        {
            try
            {
                if (channel.IsOpen)
                    channel.CloseAsync(cancellationToken: cancellationToken).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error closing RabbitMQ channel");
            }
        }

        _consumerChannels.Clear();

        logger.LogInformation("RabbitMQ Subscription is stopping...");

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var (_, channel) in _consumerChannels)
        {
            try
            {
                channel.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        _connection?.Dispose();
        _serviceScope?.Dispose();
        _serviceScope = null;

        logger.LogInformation("RabbitMQ Background Subscription Service is now stopped.");
    }
}