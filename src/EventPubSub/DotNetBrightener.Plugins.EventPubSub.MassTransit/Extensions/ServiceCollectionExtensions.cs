using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.MassTransit;
using DotNetBrightener.Plugins.EventPubSub.MassTransit.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;


public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Enables MassTransit as EventPubSub service for the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static IMassTransitConfigurator EnableMassTransit(this EventPubSubServiceBuilder builder,
                                                             string                         appName = null)
    {
        var configurator = new MassTransitConfigurator
        {
            Services = builder.Services,
            Builder = builder
        };

        builder.Services.AddSingleton<IMassTransitConfigurator>(configurator);

        if (!string.IsNullOrWhiteSpace(appName))
        {
            configurator.AppName = appName;
            configurator.NameFormatter = new SubscriptionBasedEndpointNameFormatter(appName);
        }

        return configurator;
    }

    /// <summary>
    ///     Tells the MassTransit to specify the subscription name for the application to the message broker.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="subscriptionName">The name of the application, to use as subscription name</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IMassTransitConfigurator SetSubscriptionName(this IMassTransitConfigurator builder,
                                                               string subscriptionName)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        configurator.AppName       = subscriptionName;
        configurator.NameFormatter = new SubscriptionBasedEndpointNameFormatter(subscriptionName);

        return builder;
    }

    /// <summary>
    ///     By default, MassTransit uses the full namespace of the event message type as the entity name.
    ///
    ///     Call this method to tell MassTransit to exclude the namespace from the entity name.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IMassTransitConfigurator ExcludeNamespaceInEntityName(this IMassTransitConfigurator builder)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        configurator.EntityNameFormatter = new PureClassEntityNameFormatter();

        return builder;
    }
    
    public static IMassTransitConfigurator AddConsumer<TConsumer>(this IMassTransitConfigurator builder)
        where TConsumer : class, IConsumer
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        configurator.ConfigureConsumers.Add(busConfigurator =>
        {
            busConfigurator.AddConsumer<TConsumer>();
        });

        return builder;
    }

    public static IMassTransitConfigurator AddConsumers(this IMassTransitConfigurator builder, Assembly scannedAssembly)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        configurator.ConfigureConsumers.Add(busConfigurator =>
        {
            busConfigurator.AddConsumers(scannedAssembly);
        });

        return builder;
    }

    /// <summary>
    ///     Finalizes the MassTransit configuration and registers the required services.
    ///     <br />
    ///     This method should be called right before <c>services.BuildServiceProvider()</c> or <c>builder.Build()</c>.
    ///     Otherwise, some event handlers may not be registered properly.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Finalize(this IMassTransitConfigurator builder)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        var services = configurator.Services;

        services.Replace(ServiceDescriptor.Scoped<IEventPublisher, MassTransitEventPublisher>());

        configurator.Builder
                    .EventMessageTypes
                    .Where(type => !type.IsInterface && (
                                                            type.IsAssignableTo(typeof(DistributedEventMessage)) ||
                                                            type.IsAssignableTo(typeof(IRequestMessage))
                                                        ))
                    .ToList()
                    .ForEach(msgType => ConfigureConsumerForMessageType(msgType, services, configurator));

        builder.Services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetEndpointNameFormatter(configurator.NameFormatter);

            configurator.ConfigureConsumers.ForEach(configure =>
            {
                configure.Invoke(busConfigurator);
            });

            configurator.ConfigureTransports.ForEach(configure =>
            {
                configure.Invoke(busConfigurator);
            });
        });
    }

    private static void ConfigureConsumerForMessageType(Type                    type,
                                                        IServiceCollection      services,
                                                        MassTransitConfigurator configurator)
    {
        var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(type);

        var eventHandlerTypeRegistrations = services
                                           .Where(descriptor => descriptor.ServiceType == eventHandlerType || 
                                                                descriptor.ImplementationType?.IsAssignableTo(eventHandlerType) == true)
                                           .ToList();

        var isRequestType = type.IsAssignableTo(typeof(IRequestMessage));

        if (!eventHandlerTypeRegistrations.Any())
            return;


        var responseRequestHandlerType = typeof(RequestResponder<>).MakeGenericType(type);

        if (isRequestType)
        {
            var implementations = eventHandlerTypeRegistrations
                                 .Where(descriptor => descriptor.ImplementationType != null &&
                                                      descriptor.ImplementationType!
                                                                .IsAssignableTo(responseRequestHandlerType))
                                 .Select(descriptor => descriptor.ImplementationType)
                                 .Distinct()
                                 .ToList();

            if (implementations.Count == 0)
            {
                return;
            }

            if (implementations.Count > 1)
            {
                throw new
                    InvalidOperationException($"Request type {type.Name} should only have one handler in one application.");
            }
        }

        var consumerType = isRequestType
                               ? typeof(ResponseToRequestEventHandler<>).MakeGenericType(type)
                               : typeof(ConsumerEventHandler<>).MakeGenericType(type);

        configurator.ConfigureConsumers.Add(busConfigurator =>
        {
            busConfigurator.AddConsumer(consumerType);
        });
    }
}