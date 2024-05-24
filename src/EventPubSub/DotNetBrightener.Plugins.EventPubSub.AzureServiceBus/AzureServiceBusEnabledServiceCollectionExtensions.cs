using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

public static class AzureServiceBusEnabledServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Azure Service Bus as a message publisher / subscriber service to the <see cref="EventPubSubServiceBuilder"/>'s <seealso cref="IServiceCollection"/>
    /// </summary>
    /// <param name="builder">
    ///     The <seealso cref="EventPubSubServiceBuilder"/>
    /// </param>
    /// <param name="connectionString">
    ///     The Azure Service Bus connection string
    /// </param>
    /// <param name="subscriptionName">
    ///     The name of subscription to use for the Azure Service Bus
    /// </param>
    /// <returns>
    ///     The same <seealso cref="EventPubSubServiceBuilder"/> for chaining operations
    /// </returns>
    public static EventPubSubServiceBuilder AddAzureServiceBus(this EventPubSubServiceBuilder builder,
                                                               string                         connectionString,
                                                               string                         subscriptionName = null)
    {
        AddAzureServiceBus(builder.Services, connectionString, subscriptionName);

        return builder;
    }

    /// <summary>
    ///     Adds Azure Service Bus as a message publisher / subscriber service to the specified <seealso cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <param name="connectionString">
    ///     The Azure Service Bus connection string
    /// </param>
    /// <param name="subscriptionName">
    ///     The name of subscription to use for the Azure Service Bus
    /// </param>
    /// <returns>
    ///     The same <see cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection serviceCollection,
                                                        string                  connectionString,
                                                        string                  subscriptionName = null)
    {
        serviceCollection.Configure<ServiceBusConfiguration>(c =>
        {
            c.ConnectionString = connectionString;
        });

        return AddAzureServiceBus(serviceCollection, (IConfiguration)null, subscriptionName);
    }

    public static EventPubSubServiceBuilder AddAzureServiceBus(this EventPubSubServiceBuilder builder,
                                                               IConfiguration                 configuration,
                                                               string                         subscriptionName = null)
    {
        var serviceCollection = builder.Services;

        AddAzureServiceBus(serviceCollection, configuration, subscriptionName);

        return builder;
    }

    public static IServiceCollection AddAzureServiceBus(this IServiceCollection serviceCollection,
                                                        IConfiguration          configuration,
                                                        string                  subscriptionName = null)
    {

        var handlerMapping = new AzureServiceBusHandlerMapping();
        serviceCollection.AddSingleton(handlerMapping);

        serviceCollection.AddScoped<IServiceBusMessagePublisher, ServiceBusMessagePublisher>();
        serviceCollection.AddScoped<IAzureServiceBusHelperService, AzureServiceBusHelperService>();
        serviceCollection.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor<SimpleAzureEventMessage>>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<IEventPublisher, AzureServiceBusEnabledEventPublisher>());

        serviceCollection.AddScoped(typeof(AzureServiceBusEventSubscription<>));

        if (configuration is not null)
            serviceCollection
               .Configure<ServiceBusConfiguration>(configuration.GetSection(nameof(ServiceBusConfiguration)));

        serviceCollection.Configure<ServiceBusConfiguration>(c =>
        {
            if (serviceCollection.FirstOrDefault(_ => _.ServiceType == typeof(EventPubSubServiceBuilder) &&
                                                      _.ImplementationInstance is not null)
                                ?.ImplementationInstance is not EventPubSubServiceBuilder builder)
            {
                throw new
                    InvalidOperationException("Failed to register Azure Service Bus: EventPubSubServiceBuilder is not initialized within the ServiceCollection");
            }

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

                handlerMapping.TryAdd(eventMessageType, consumerType);
            }

            if (errors.Any())
                throw new
                    InvalidOperationException($"Failed to register Azure Service Bus: {string.Join(", ", errors)}");
        });

        serviceCollection.Configure<ServiceBusConfiguration>(c =>
        {
            if (string.IsNullOrEmpty(c.SubscriptionName) &&
                !string.IsNullOrEmpty(subscriptionName))
                c.SubscriptionName = subscriptionName;

            if (string.IsNullOrEmpty(c.ConnectionString))
                throw new InvalidOperationException("Azure Service Bus ConnectionString must be provided");

            if (string.IsNullOrEmpty(c.SubscriptionName))
                throw new InvalidOperationException("SubscriptionName must be provided");
        });

        serviceCollection.AddHostedService<AzureServiceBusSubscribeHostedService>();

        return serviceCollection;
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