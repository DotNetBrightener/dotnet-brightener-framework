using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

public static class AzureServiceBusEnabledServiceCollectionExtensions
{
    public static EventPubSubServiceBuilder AddAzureServiceBus(this EventPubSubServiceBuilder builder,
                                                               string                         connectionString,
                                                               string                         subscriptionName = null)
    {
        builder.Services.Configure<ServiceBusConfiguration>(c =>
        {
            c.ConnectionString = connectionString;
        });

        return AddAzureServiceBus(builder, (IConfiguration)null, subscriptionName);
    }

    public static EventPubSubServiceBuilder AddAzureServiceBus(this EventPubSubServiceBuilder builder,
                                                               IConfiguration                 configuration,
                                                               string                         subscriptionName = null)
    {
        if (configuration is not null)
            builder.Services
                   .Configure<ServiceBusConfiguration>(configuration.GetSection(nameof(ServiceBusConfiguration)));

        builder.Services.Configure<ServiceBusConfiguration>(c =>
        {
            if (string.IsNullOrEmpty(c.SubscriptionName) &&
                !string.IsNullOrEmpty(subscriptionName))
                c.SubscriptionName = subscriptionName;

            if (string.IsNullOrEmpty(c.ConnectionString))
                throw new InvalidOperationException("Azure Service Bus ConnectionString must be provided");

            if (string.IsNullOrEmpty(c.SubscriptionName))
                throw new InvalidOperationException("SubscriptionName must be provided");
        });


        var handlerMapping = new AzureServiceBusHandlerMapping();
        builder.Services.AddSingleton(handlerMapping);

        builder.Services.AddScoped<IServiceBusMessagePublisher, ServiceBusMessagePublisher>();
        builder.Services.AddScoped<IAzureServiceBusHelperService, AzureServiceBusHelperService>();
        builder.Services.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor<SimpleAzureEventMessage>>();

        builder.Services.Replace(ServiceDescriptor.Scoped<IEventPublisher, AzureServiceBusEnabledEventPublisher>());

        var distributedEventMessageTypes = builder.EventMessageTypes
                                                  .Where(evtMsgType => evtMsgType is not null &&
                                                                       evtMsgType
                                                                          .IsAssignableTo(typeof(
                                                                                              IDistributedEventMessage)));
        var errors = new List<string>();

        foreach (var eventMessageType in distributedEventMessageTypes)
        {
            var topicName = eventMessageType.GetTopicName();

            if (topicName.Length > 260)
            {
                errors.Add($"Topic name {topicName} for event message type {eventMessageType.FullName} is too long. " +
                           $"Must be less than 260 characters. " +
                           $"Adjust the namespace of the event message type will help.");
                continue;
            }

            var consumerType = typeof(AzureServiceBusEventSubscription<>).MakeGenericType(eventMessageType);

            handlerMapping.Add(eventMessageType, consumerType);

            builder.Services.AddScoped(consumerType);
        }

        if (errors.Any())
            throw new InvalidOperationException($"Failed to register Azure Service Bus: {string.Join(", ", errors)}");

        builder.Services.AddHostedService<AzureServiceBusSubscribeHostedService>();

        return builder;
    }

    /// <summary>
    ///     Uses a custom message type for the Azure Service Bus message decoration
    /// </summary>
    /// <remarks>
    ///     During ConfigureServices phases of the application, only the last call to this method will take effect.
    /// </remarks>>
    /// <typeparam name="TMessageType">
    ///     The type of the topic message that will be sent to / received from the Azure Service Bus
    /// </typeparam>
    public static EventPubSubServiceBuilder UseDecorationMessageType<TMessageType>(
        this EventPubSubServiceBuilder builder)
        where TMessageType : AzureServiceBusEventMessage
    {
        builder.Services.Replace(ServiceDescriptor
                                    .Scoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor<TMessageType>>());

        return builder;
    }
}