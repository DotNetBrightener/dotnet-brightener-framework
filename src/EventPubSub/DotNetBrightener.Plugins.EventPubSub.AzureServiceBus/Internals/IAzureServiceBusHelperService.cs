using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal interface IAzureServiceBusHelperService
{
    Task CreateTopicIfNotExists(string topicName, CancellationToken? cancellationToken = default);

    Task CreateSubscriptionIfNotExists(string topicName,
                                       string subscriptionName,
                                       CancellationToken? cancellationToken = default);

    Task CreateReceiverQueue(string topicName, string subscriptionName);
}

internal class AzureServiceBusHelperService(IOptions<ServiceBusConfiguration> serviceBusConfiguration)
    : IAzureServiceBusHelperService
{
    private const bool RequiresDuplicateDetection = true;

    public async Task CreateTopicIfNotExists(string topicName, CancellationToken? cancellationToken = default)
    {
        var adminClient = new ServiceBusAdministrationClient(serviceBusConfiguration.Value.ConnectionString);

        var topicExists = await adminClient.TopicExistsAsync(topicName, cancellationToken ?? CancellationToken.None);

        if (topicExists)
            return;

        var td = new CreateTopicOptions(topicName)
        {
            AutoDeleteOnIdle                    = serviceBusConfiguration.Value.AutoDeleteOnIdle,
            DefaultMessageTimeToLive            = serviceBusConfiguration.Value.DefaultMessageTimeToLive,
            DuplicateDetectionHistoryTimeWindow = serviceBusConfiguration.Value.DuplicateDetectionHistoryTimeWindow,
            MaxSizeInMegabytes                  = serviceBusConfiguration.Value.MaxSizeInMegabytes,
            RequiresDuplicateDetection          = RequiresDuplicateDetection
        };

        var result = await adminClient.CreateTopicAsync(td);

        if (result.Value == null)
            throw new Exception($"Failed to create topic {topicName}");
    }

    public async Task CreateSubscriptionIfNotExists(string             topicName,
                                                    string             subscriptionName,
                                                    CancellationToken? cancellationToken = default)
    {
        var adminClient = new ServiceBusAdministrationClient(serviceBusConfiguration.Value.ConnectionString);

        var topicExists =
            await adminClient.SubscriptionExistsAsync(topicName,
                                                      subscriptionName,
                                                      cancellationToken ?? CancellationToken.None);

        if (topicExists)
            return;


        var subscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName)
        {
            ForwardTo = null
        };

        var result =
            await adminClient.CreateSubscriptionAsync(subscriptionOptions, cancellationToken ?? CancellationToken.None);

        if (result.Value == null)
            throw new Exception($"Failed to create subscription {subscriptionName} for topic {topicName}");
    }

    public async Task CreateReceiverQueue(string topicName, string subscriptionName)
    {
        var queueName = $"receiver-{subscriptionName}-{topicName}";

        var adminClient = new ServiceBusAdministrationClient(serviceBusConfiguration.Value.ConnectionString);

        if (!await adminClient.QueueExistsAsync(queueName))
        {
            var options = new CreateQueueOptions(queueName)
            {
                ForwardTo = null
            };

            await adminClient.CreateQueueAsync(options);
        }
    }
}