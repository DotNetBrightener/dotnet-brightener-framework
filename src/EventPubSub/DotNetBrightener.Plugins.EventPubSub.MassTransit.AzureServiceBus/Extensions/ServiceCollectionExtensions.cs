using DotNetBrightener.Plugins.EventPubSub.MassTransit;
using DotNetBrightener.Plugins.EventPubSub.MassTransit.AzureServiceBus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;


public static class ServiceCollectionExtensions
{
    public static IMassTransitConfigurator UseAzureServiceBus(this IMassTransitConfigurator builder,
                                                              IConfiguration                configuration)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        if (configurator.ConfigureTransports.Any())
            throw new
                InvalidOperationException($"Only one transport can be used in an application. Remove all other transports and only enable one that you need to use.");


        ServiceBusConfiguration serviceBusConfiguration = configuration.GetSection(nameof(ServiceBusConfiguration))
                                                                       .Get<ServiceBusConfiguration>();

        builder.Services.AddSingleton(serviceBusConfiguration);

        builder.SetSubscriptionName(serviceBusConfiguration.SubscriptionName);

        if (!serviceBusConfiguration.IncludeNamespaceForTopicName)
        {
            builder.ExcludeNamespaceInEntityName();
        }

        return builder.UseAzureServiceBus(serviceBusConfiguration.ConnectionString);
    }

    public static IMassTransitConfigurator UseAzureServiceBus(this IMassTransitConfigurator builder,
                                                              string connectionString)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        if (configurator.ConfigureTransports.Any())
            throw new
                InvalidOperationException($"Only one transport can be used in an application. Remove all other transports and only enable one that you need to use.");

        configurator.ConfigureTransports.Add(busConfigurator =>
        {
            busConfigurator.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(connectionString);

                if (configurator.EntityNameFormatter is not null)
                    cfg.MessageTopology.SetEntityNameFormatter(configurator.EntityNameFormatter);

                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}