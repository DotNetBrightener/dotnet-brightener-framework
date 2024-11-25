using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.Distributed;
using DotNetBrightener.Plugins.EventPubSub.Distributed.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;


public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Enables integration with distributed message brokers the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static IDistributedEventPubSubConfigurator EnableDistributedIntegrations(
        this EventPubSubServiceBuilder builder,
        string                         appName = null)
    {
        var configurator = new DistributedIntegrationsConfigurator
        {
            Services = builder.Services,
            Builder  = builder
        };

        builder.Services.AddSingleton<IDistributedEventPubSubConfigurator>(configurator);

        if (!string.IsNullOrWhiteSpace(appName))
        {
            configurator.AppName       = appName;
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
    public static IDistributedEventPubSubConfigurator SetSubscriptionName(
        this IDistributedEventPubSubConfigurator builder,
        string                                   subscriptionName)
    {
        if (builder is not DistributedIntegrationsConfigurator configurator)
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
    public static IDistributedEventPubSubConfigurator ExcludeNamespaceInEntityName(
        this IDistributedEventPubSubConfigurator builder)
    {
        if (builder is not DistributedIntegrationsConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        configurator.EntityNameFormatter = new PureClassEntityNameFormatter();

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
    public static void Finalize(this IDistributedEventPubSubConfigurator builder)
    {
        if (builder is not DistributedIntegrationsConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        var services = configurator.Services;

        services.Replace(ServiceDescriptor.Scoped<IEventPublisher, DistributedEventPublisher>());


        var consumerHandlers = builder.Services
                                      .Where(x => x.ImplementationType is { BaseType.IsGenericType: true } &&
                                                  x.ImplementationType.IsAssignableTo(typeof(IEventHandler)) &&
                                                  x.ImplementationType.GetInterfaces()
                                                   .Any(@interface => @interface.IsGenericType &&
                                                                      @interface.GetGenericTypeDefinition() ==
                                                                      typeof(IConsumer<>)))
                                      .Select(x => x.ImplementationType)
                                      .Distinct()
                                      .ToArray();

        Console.WriteLine($"Found {consumerHandlers.Length} consumer types");

        builder.Services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetEndpointNameFormatter(configurator.NameFormatter);
            
            foreach (var consumerHandler in consumerHandlers)
            {
                busConfigurator.AddConsumer(consumerHandler);
            }

            configurator.ConfigureTransports.ForEach(configure =>
            {
                configure.Invoke(busConfigurator, consumerHandlers);
            });
        });
    }
}