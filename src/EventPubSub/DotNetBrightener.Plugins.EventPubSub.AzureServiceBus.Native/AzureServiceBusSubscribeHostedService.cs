using System.Collections.Concurrent;
using System.Text;
using Azure.Messaging.ServiceBus;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native;

internal class AzureServiceBusSubscribeHostedService(
    ILogger<AzureServiceBusSubscribeHostedService> logger,
    IOptions<ServiceBusConfiguration>              serviceBusConfiguration,
    AzureServiceBusHandlerMapping                  eventBusHandlerTypesMapping,
    IServiceScopeFactory                           serviceScopeFactory,
    ILoggerFactory                                 loggerFactory)
    : IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<Type, ServiceBusProcessor> _serviceBusProcessors = new();
    private readonly ILogger _logger = loggerFactory.CreateLogger<AzureServiceBusSubscribeHostedService>();
    private readonly string _subscriptionName = serviceBusConfiguration.Value.SubscriptionName;

    private          IServiceScope    _serviceScope;
    private readonly ServiceBusClient _serviceBusClient = new(serviceBusConfiguration.Value.ConnectionString);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Azure Service Bus Background Subscription Service is starting...");

        _serviceScope = serviceScopeFactory.CreateScope();
        var serviceBusService = _serviceScope.ServiceProvider.GetRequiredService<IAzureServiceBusHelperService>();

        await eventBusHandlerTypesMapping.ParallelForEachAsync(async pair =>
        {
            var (messageType, handlerType) = pair;

            var busProcessor = await CreateBusProcessor(messageType,
                                                        handlerType,
                                                        serviceBusService,
                                                        cancellationToken);

            _serviceBusProcessors.TryAdd(messageType, busProcessor);
        });
    }

    private async Task<ServiceBusProcessor> CreateBusProcessor(Type                          messageType,
                                                               Type                          handlerType,
                                                               IAzureServiceBusHelperService serviceBusService,
                                                               CancellationToken             cancellationToken)
    {
        var topicName = messageType.GetTopicName();

        await serviceBusService.CreateTopicIfNotExists(topicName);
        await serviceBusService.CreateSubscriptionIfNotExists(topicName, _subscriptionName);

        var messageHandlerOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls   = 1,
            AutoCompleteMessages = false
        };

        var busProcessor = _serviceBusClient.CreateProcessor(topicName, _subscriptionName, messageHandlerOptions);
        busProcessor.ProcessMessageAsync += args => ProcessMessage(args, handlerType);
        busProcessor.ProcessErrorAsync   += args => OnFailure(args, topicName, _subscriptionName);

        logger.LogInformation("Subscription {subscriptionName} for topic {topicName} is subscribing...",
                              _subscriptionName,
                              topicName);
        
        await busProcessor.StartProcessingAsync(cancellationToken);

        logger.LogInformation("Subscribe successfully. Subscription {subscriptionName} for topic {topicName} is listening",
                              _subscriptionName,
                              topicName);

        return busProcessor;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Signalling Service Bus Processors to stop...");

        foreach (var (_, processor) in _serviceBusProcessors)
        {
            if (!processor.IsClosed)
            {
                await processor.CloseAsync(cancellationToken);
            }
        }

        _serviceBusProcessors.Clear();

        logger.LogInformation("Service Bus Subscription is stopping...");
    }

    async Task ProcessMessage(ProcessMessageEventArgs args, Type handlerType)
    {
        var bytesAsString = Encoding.UTF8.GetString(args.Message.Body);

        using var scope = serviceScopeFactory.CreateScope();

        var messageProcessor = scope.ServiceProvider.GetRequiredService<IServiceBusMessageProcessor>();

        var domainEvent = await messageProcessor.ParseIncomingMessage(bytesAsString);

        if (scope.ServiceProvider.GetRequiredService(handlerType) is IAzureServiceBusEventSubscription handler)
        {
            var shouldComplete = await handler.ProcessMessage(domainEvent);

            if (shouldComplete)
                await args.CompleteMessageAsync(args.Message);
        }
    }


    async Task OnFailure(ProcessErrorEventArgs args, string topicName, string subscriptionName)
    {
        _logger?.LogError(args.Exception,
                          "Error occured while processing message of topic {topicName} from {subscription}",
                          topicName,
                          subscriptionName);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _serviceScope = null;
        logger.LogInformation("Azure Service Bus Background Subscription Service is now stopped.");
    }
}