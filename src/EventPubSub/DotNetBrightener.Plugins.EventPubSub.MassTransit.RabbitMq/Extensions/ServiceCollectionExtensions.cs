﻿using DotNetBrightener.Plugins.EventPubSub.MassTransit;
using DotNetBrightener.Plugins.EventPubSub.MassTransit.RabbitMq;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;


public static class ServiceCollectionExtensions
{
    public static IMassTransitConfigurator UseRabbitMq(this IMassTransitConfigurator builder,
                                                       IConfiguration                configuration)
    {
        if (builder is not MassTransitConfigurator configurator)
        {
            throw new InvalidOperationException("Invalid configurator type");
        }

        if (configurator.ConfigureTransports.Any())
            throw new
                InvalidOperationException($"Only one transport can be used in an application. Remove all other transports and only enable one that you need to use.");


        RabbitMqConfiguration rabbitMqConfiguration = configuration.GetSection(nameof(RabbitMqConfiguration))
                                                                   .Get<RabbitMqConfiguration>();

        builder.Services.AddSingleton(rabbitMqConfiguration);

        builder.SetSubscriptionName(rabbitMqConfiguration.SubscriptionName);

        if (!rabbitMqConfiguration.IncludeNamespaceForTopicName)
        {
            builder.ExcludeNamespaceInEntityName();
        }

        return builder.UseRabbitMq(rabbitMqConfiguration.Host,
                                   rabbitMqConfiguration.Username,
                                   rabbitMqConfiguration.Password,
                                   rabbitMqConfiguration.VirtualHost);
    }

    public static IMassTransitConfigurator UseRabbitMq(this IMassTransitConfigurator builder,
                                                       string host,
                                                       string username = "",
                                                       string password = "",
                                                       string virtualHost = "/")
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
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host,
                         virtualHost,
                         h =>
                         {
                             if (!string.IsNullOrWhiteSpace(username))
                                 h.Username(username);

                             if (!string.IsNullOrWhiteSpace(password))
                                 h.Password(password);
                         });

                if (configurator.EntityNameFormatter is not null)
                    cfg.MessageTopology.SetEntityNameFormatter(configurator.EntityNameFormatter);

                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}