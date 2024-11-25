using MassTransit;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed;

/// <summary>
///     Defines the formatter that will use subscription name as name of the consumer.
/// </summary>
/// <param name="subscriptionName">
///     The name of the subscription, which is the application that subscribes to the message.
/// </param>
internal class SubscriptionBasedEndpointNameFormatter(string subscriptionName = "") : DefaultEndpointNameFormatter
{
    public string SubscriptionName { get; init; } = subscriptionName;

    public override string Consumer<T>()
    {
        if (!string.IsNullOrWhiteSpace(SubscriptionName))
            return SubscriptionName;

        var consumer = base.Consumer<T>();

        return consumer;
    }
}