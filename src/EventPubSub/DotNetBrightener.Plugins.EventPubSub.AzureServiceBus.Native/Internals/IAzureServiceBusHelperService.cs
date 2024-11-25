using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Internals;

internal interface IAzureServiceBusHelperService
{
    Task CreateTopicIfNotExists(string topicName, CancellationToken? cancellationToken = default);

    Task CreateSubscriptionIfNotExists(string topicName,
                                       string subscriptionName,
                                       CancellationToken? cancellationToken = default);
}

internal class AzureServiceBusHelperService(IOptions<ServiceBusConfiguration> serviceBusConfiguration) : IAzureServiceBusHelperService
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
            AutoDeleteOnIdle = serviceBusConfiguration.Value.AutoDeleteOnIdle,
            DefaultMessageTimeToLive = serviceBusConfiguration.Value.DefaultMessageTimeToLive,
            DuplicateDetectionHistoryTimeWindow = serviceBusConfiguration.Value.DuplicateDetectionHistoryTimeWindow,
            MaxSizeInMegabytes = serviceBusConfiguration.Value.MaxSizeInMegabytes,
            RequiresDuplicateDetection = RequiresDuplicateDetection
        };

        var result = await adminClient.CreateTopicAsync(td);

        if (result.Value == null)
            throw new Exception($"Failed to create topic {topicName}");
    }

    public async Task CreateSubscriptionIfNotExists(string topicName,
                                                    string subscriptionName,
                                                    CancellationToken? cancellationToken = default)
    {
        var adminClient = new ServiceBusAdministrationClient(serviceBusConfiguration.Value.ConnectionString);

        var topicExists = await adminClient.SubscriptionExistsAsync(topicName, subscriptionName, cancellationToken ?? CancellationToken.None);

        if (topicExists)
            return;


        var subscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
        var result =
            await adminClient.CreateSubscriptionAsync(subscriptionOptions, cancellationToken ?? CancellationToken.None);

        if (result.Value == null)
            throw new Exception($"Failed to create subscription {subscriptionName} for topic {topicName}");
    }
}