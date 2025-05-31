using System.Collections.Concurrent;
using System.Text;
using Azure.Messaging.ServiceBus;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

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

            if (busProcessor is not null)
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

        if (messageType.IsAssignableTo(typeof(IResponseMessage)))
        {
            await serviceBusService.CreateReceiverQueue(topicName, _subscriptionName);
        }

        if (handlerType is null)
            return null;

        var messageHandlerOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls   = 1,
            AutoCompleteMessages = false,
        };

        var busProcessor = _serviceBusClient.CreateProcessor(topicName, _subscriptionName, messageHandlerOptions);
        busProcessor.ProcessMessageAsync += args => ProcessMessage(args, handlerType, _serviceBusClient);
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

    private async Task ProcessMessage(ProcessMessageEventArgs args, Type handlerType, ServiceBusClient serviceBusClient)
    {
        var bytesAsString = Encoding.UTF8.GetString(args.Message.Body);

        using var scope = serviceScopeFactory.CreateScope();

        var messageProcessor = scope.ServiceProvider.GetRequiredService<IServiceBusMessageProcessor>();

        var domainEvent = await messageProcessor.ParseIncomingMessage(bytesAsString);

        if (scope.ServiceProvider.TryGet(handlerType) is IAzureServiceBusEventSubscription handler)
        {
            domainEvent.CurrentApp = _subscriptionName;
            var shouldComplete = await handler.ProcessMessage(domainEvent, args);

            if (shouldComplete)
                await args.CompleteMessageAsync(args.Message);
        }

        if (scope.ServiceProvider.TryGet(handlerType) is IAzureServiceBusEventRequestResponseHandler responder)
        {
            domainEvent.CurrentApp = _subscriptionName;
            var shouldComplete = await responder.ProcessMessage(domainEvent, args, serviceBusClient);

            if (shouldComplete)
                await args.CompleteMessageAsync(args.Message);
        }
    }


    private async Task OnFailure(ProcessErrorEventArgs args, string topicName, string subscriptionName)
    {
        _logger?.LogError(args.Exception,
                          "Error occured while processing message of topic {topicName} from {subscription}",
                          topicName,
                          subscriptionName);
    }

    public void Dispose()
    {
        _serviceScope?.Dispose();
        _serviceScope = null;
        logger.LogInformation("Azure Service Bus Background Subscription Service is now stopped.");
    }
}