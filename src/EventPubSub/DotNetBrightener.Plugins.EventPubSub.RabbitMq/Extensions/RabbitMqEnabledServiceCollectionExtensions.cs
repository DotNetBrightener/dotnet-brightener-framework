using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.Plugins.EventPubSub.Distributed;
using DotNetBrightener.Plugins.EventPubSub.RabbitMq;
using DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class RabbitMqEnabledServiceCollectionExtensions
{
    /// <summary>
    ///     Adds RabbitMQ as a message publisher / subscriber service to the
    ///     <see cref="EventPubSubServiceBuilder"/>'s <seealso cref="IServiceCollection"/>
    /// </summary>
    /// <param name="builder">
    ///     The <seealso cref="EventPubSubServiceBuilder"/>
    /// </param>
    /// <param name="hostName">
    ///     The RabbitMQ server hostname
    /// </param>
    /// <param name="subscriptionName">
    ///     The name of subscription (queue consumer identifier) to use for RabbitMQ
    /// </param>
    /// <returns>
    ///     The same <seealso cref="EventPubSubServiceBuilder"/> for chaining operations
    /// </returns>
    public static EventPubSubServiceBuilder AddRabbitMq(this EventPubSubServiceBuilder builder,
                                                        string                         hostName,
                                                        string                         subscriptionName = null)
    {
        builder.Services.AddRabbitMq(hostName, subscriptionName);

        return builder;
    }

    /// <summary>
    ///     Adds RabbitMQ as a message publisher / subscriber service to the specified <seealso cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <param name="hostName">
    ///     The RabbitMQ server hostname
    /// </param>
    /// <param name="subscriptionName">
    ///     The name of subscription (queue consumer identifier) to use for RabbitMQ
    /// </param>
    /// <returns>
    ///     The same <seealso cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection AddRabbitMq(this IServiceCollection serviceCollection,
                                                 string                  hostName,
                                                 string                  subscriptionName = null)
    {
        serviceCollection.Configure<RabbitMqConfiguration>(c =>
        {
            c.HostName = hostName;
        });

        return serviceCollection.AddRabbitMq((IConfiguration)null, subscriptionName);
    }

    /// <summary>
    ///     Adds RabbitMQ as a message publisher / subscriber service using configuration from <seealso cref="IConfiguration"/>
    /// </summary>
    public static EventPubSubServiceBuilder AddRabbitMq(this EventPubSubServiceBuilder builder,
                                                        IConfiguration                 configuration,
                                                        string                         subscriptionName = null)
    {
        var serviceCollection = builder.Services;

        serviceCollection.AddRabbitMq(configuration, subscriptionName);

        return builder;
    }

    /// <summary>
    ///     Adds RabbitMQ as a message publisher / subscriber service using configuration from <seealso cref="IConfiguration"/>
    /// </summary>
    public static IServiceCollection AddRabbitMq(this IServiceCollection serviceCollection,
                                                 IConfiguration          configuration,
                                                 string                  subscriptionName = null)
    {
        var handlerMapping = new RabbitMqHandlerMapping();
        serviceCollection.AddSingleton(handlerMapping);

        serviceCollection.AddScoped<IDistributedMessagePublisher, DistributedMessagePublisher>();
        serviceCollection.AddScoped<IRabbitMqHelperService, RabbitMqHelperService>();
        serviceCollection.AddScoped<IRabbitMqMessageProcessor,
            DefaultRabbitMqMessageProcessor<SimpleRabbitMqEventMessageWrapper>>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<IEventPublisher, DistributedEventPublisher>());

        serviceCollection.AddScoped(typeof(RabbitMqEventSubscription<>));

        if (configuration is not null)
            serviceCollection
               .Configure<RabbitMqConfiguration>(configuration.GetSection(nameof(RabbitMqConfiguration)));

        serviceCollection.Configure<RabbitMqConfiguration>(c =>
        {
            if (serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(EventPubSubServiceBuilder) &&
                                                      d.ImplementationInstance is not null)
                                ?.ImplementationInstance is not EventPubSubServiceBuilder builder)
            {
                throw new
                    InvalidOperationException("Failed to register RabbitMQ: EventPubSubServiceBuilder is not initialized within the ServiceCollection");
            }

            if (string.IsNullOrEmpty(c.SubscriptionName) &&
                !string.IsNullOrEmpty(subscriptionName))
                c.SubscriptionName = subscriptionName;

            if (string.IsNullOrEmpty(c.HostName))
                throw new InvalidOperationException("RabbitMQ HostName must be provided");

            if (string.IsNullOrEmpty(c.SubscriptionName))
                throw new InvalidOperationException("SubscriptionName must be provided");

            TypeToRabbitMqExchangeNameUtil.RabbitMqConfiguration = c;

            var distributedEventMessageTypes = builder.EventMessageTypes
                                                      .Where(evtMsgType => evtMsgType is not null &&
                                                                           evtMsgType
                                                                              .IsAssignableTo(typeof(
                                                                                                  DistributedEventMessage)));

            var errors = new List<string>();

            foreach (var eventMessageType in distributedEventMessageTypes)
            {
                var exchangeName = eventMessageType.GetExchangeName();

                var consumerType = typeof(RabbitMqEventSubscription<>).MakeGenericType(eventMessageType);

                handlerMapping.TryAdd(eventMessageType, consumerType);
            }

            var requestTypes = builder.EventMessageTypes
                                      .Where(evtMsgType => evtMsgType is not null &&
                                                           evtMsgType
                                                              .IsAssignableTo(typeof(RequestMessage)));

            foreach (var eventMessageType in requestTypes)
            {
                var exchangeName = eventMessageType.GetExchangeName();

                var consumerType = typeof(RabbitMqEventRequestResponseHandler<>).MakeGenericType(eventMessageType);

                handlerMapping.TryAdd(eventMessageType, consumerType);
            }

            var responseTypes = builder.EventMessageTypes
                                       .Where(evtMsgType => evtMsgType is not null &&
                                                            evtMsgType
                                                               .IsAssignableTo(typeof(ResponseMessage)));

            foreach (var eventMessageType in responseTypes)
            {
                handlerMapping.TryAdd(eventMessageType, null);
            }

            if (errors.Any())
                throw new
                    InvalidOperationException($"Failed to register RabbitMQ: {string.Join(", ", errors)}");
        });

        serviceCollection.AddHostedService<RabbitMqSubscribeHostedService>();

        return serviceCollection;
    }

    /// <summary>
    ///     Uses a custom message type for the RabbitMQ message decoration
    /// </summary>
    /// <remarks>
    ///     During ConfigureServices phases of the application, only the last call to this method will take effect.
    /// </remarks>
    /// <typeparam name="TMessageType">
    ///     The type of the message wrapper that will be sent to / received from RabbitMQ
    /// </typeparam>
    public static EventPubSubServiceBuilder UseDecorationMessageType<TMessageType>(
        this EventPubSubServiceBuilder builder)
        where TMessageType : EventMessageWrapper, new()
    {
        builder.Services.UseDecorationMessageType<TMessageType>();

        return builder;
    }

    /// <summary>
    ///     Uses a custom message type for the RabbitMQ message decoration
    /// </summary>
    /// <remarks>
    ///     During ConfigureServices phases of the application, only the last call to this method will take effect.
    /// </remarks>
    /// <typeparam name="TMessageType">
    ///     The type of the message wrapper that will be sent to / received from RabbitMQ
    /// </typeparam>
    public static IServiceCollection UseDecorationMessageType<TMessageType>(this IServiceCollection services)
        where TMessageType : EventMessageWrapper, new()
    {
        services.UseDecorationMessageType<TMessageType, DefaultRabbitMqMessageProcessor<TMessageType>>();

        return services;
    }

    /// <summary>
    ///     Uses a custom message type with a custom processor for the RabbitMQ message decoration
    /// </summary>
    /// <remarks>
    ///     During ConfigureServices phases of the application, only the last call to this method will take effect.
    /// </remarks>
    /// <typeparam name="TMessageType">
    ///     The type of the message wrapper that will be sent to / received from RabbitMQ
    /// </typeparam>
    /// <typeparam name="TMessageTypeProcessor">
    ///     The processor that processes the <typeparamref name="TMessageType"/> message
    /// </typeparam>
    public static EventPubSubServiceBuilder UseDecorationMessageType<TMessageType, TMessageTypeProcessor>(
        this EventPubSubServiceBuilder builder)
        where TMessageType : EventMessageWrapper, new()
        where TMessageTypeProcessor : RabbitMqMessageProcessor<TMessageType>
    {
        builder.Services.UseDecorationMessageType<TMessageType, TMessageTypeProcessor>();

        return builder;
    }

    /// <summary>
    ///     Uses a custom message type with a custom processor for the RabbitMQ message decoration
    /// </summary>
    /// <remarks>
    ///     During ConfigureServices phases of the application, only the last call to this method will take effect.
    /// </remarks>
    /// <typeparam name="TMessageType">
    ///     The type of the message wrapper that will be sent to / received from RabbitMQ
    /// </typeparam>
    /// <typeparam name="TMessageTypeProcessor">
    ///     The processor that processes the <typeparamref name="TMessageType"/> message
    /// </typeparam>
    public static IServiceCollection UseDecorationMessageType<TMessageType, TMessageTypeProcessor>(
        this IServiceCollection services)
        where TMessageType : EventMessageWrapper, new()
        where TMessageTypeProcessor : RabbitMqMessageProcessor<TMessageType>
    {
        if (services.FirstOrDefault(d => d.ServiceType == typeof(EventPubSubServiceBuilder) &&
                                         d.ImplementationInstance is not null)
                   ?.ImplementationInstance is not EventPubSubServiceBuilder)
        {
            throw new
                InvalidOperationException("Failed to register RabbitMQ: EventPubSubServiceBuilder is not initialized within the ServiceCollection");
        }

        services.Replace(ServiceDescriptor.Scoped<IRabbitMqMessageProcessor, TMessageTypeProcessor>());

        return services;
    }
}