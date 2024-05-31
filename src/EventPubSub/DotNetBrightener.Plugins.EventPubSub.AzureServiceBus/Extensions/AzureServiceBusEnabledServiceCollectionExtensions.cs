using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

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
        builder.Services.AddAzureServiceBus(connectionString, subscriptionName);

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

        return serviceCollection.AddAzureServiceBus((IConfiguration)null, subscriptionName);
    }

    public static EventPubSubServiceBuilder AddAzureServiceBus(this EventPubSubServiceBuilder builder,
                                                               IConfiguration                 configuration,
                                                               string                         subscriptionName = null)
    {
        var serviceCollection = builder.Services;

        serviceCollection.AddAzureServiceBus(configuration, subscriptionName);

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
        serviceCollection.AddScoped<IServiceBusMessageProcessor,
            DefaultServiceBusMessageProcessor<SimpleAzureEventMessageWrapper>>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<IEventPublisher, AzureServiceBusEnabledEventPublisher>());

        serviceCollection.AddScoped(typeof(AzureServiceBusEventSubscription<>));

        if (configuration is not null)
            serviceCollection
               .Configure<ServiceBusConfiguration>(configuration.GetSection(nameof(ServiceBusConfiguration)));

        serviceCollection.Configure<ServiceBusConfiguration>(c =>
        {
            if (serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(EventPubSubServiceBuilder) &&
                                                      d.ImplementationInstance is not null)
                                ?.ImplementationInstance is not EventPubSubServiceBuilder builder)
            {
                throw new
                    InvalidOperationException("Failed to register Azure Service Bus: EventPubSubServiceBuilder is not initialized within the ServiceCollection");
            }


            if (string.IsNullOrEmpty(c.SubscriptionName) &&
                !string.IsNullOrEmpty(subscriptionName))
                c.SubscriptionName = subscriptionName;

            if (string.IsNullOrEmpty(c.ConnectionString))
                throw new InvalidOperationException("Azure Service Bus ConnectionString must be provided");

            if (string.IsNullOrEmpty(c.SubscriptionName))
                throw new InvalidOperationException("SubscriptionName must be provided");

            TypeToAzureTopicNameUtil.ServiceBusConfiguration = c;

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
        where TMessageType : EventMessageWrapper
    {
        builder.Services.UseDecorationMessageType<TMessageType>();

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
    public static IServiceCollection UseDecorationMessageType<TMessageType>(this IServiceCollection services)
        where TMessageType : EventMessageWrapper
    {
        services.UseDecorationMessageType<TMessageType, DefaultServiceBusMessageProcessor<TMessageType>>();

        return services;
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
    /// <typeparam name="TMessageTypeProcessor">
    ///     The processor that processes the <typeparamref name="TMessageType"/> message
    /// </typeparam>
    public static EventPubSubServiceBuilder UseDecorationMessageType<TMessageType, TMessageTypeProcessor>(
        this EventPubSubServiceBuilder builder)
        where TMessageType : EventMessageWrapper
        where TMessageTypeProcessor : ServiceBusMessageProcessor<TMessageType>
    {
        builder.Services.UseDecorationMessageType<TMessageType, TMessageTypeProcessor>();

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
    /// <typeparam name="TMessageTypeProcessor">
    ///     The processor that processes the <typeparamref name="TMessageType"/> message
    /// </typeparam>
    public static IServiceCollection UseDecorationMessageType<TMessageType, TMessageTypeProcessor>(
        this IServiceCollection services)
        where TMessageType : EventMessageWrapper
        where TMessageTypeProcessor : ServiceBusMessageProcessor<TMessageType>
    {
        if (services.FirstOrDefault(d => d.ServiceType == typeof(EventPubSubServiceBuilder) &&
                                         d.ImplementationInstance is not null)
                   ?.ImplementationInstance is not EventPubSubServiceBuilder)
        {
            throw new
                InvalidOperationException("Failed to register Azure Service Bus: EventPubSubServiceBuilder is not initialized within the ServiceCollection");
        }

        services.Replace(ServiceDescriptor.Scoped<IServiceBusMessageProcessor, TMessageTypeProcessor>());

        return services;
    }
}