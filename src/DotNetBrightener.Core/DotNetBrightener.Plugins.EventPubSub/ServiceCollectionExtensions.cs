using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Plugins.EventPubSub
{
    public static class ServiceCollectionEventPublisherExtensions
    {
        /// <summary>
        ///     Detects and registers the event publisher service and event handlers to the <see cref="serviceCollection"/>
        /// </summary>
        /// <param name="serviceCollection">
        ///     The <see cref="IServiceCollection"/>
        /// </param>
        /// <param name="serviceTypes">
        ///     If specified, only finds and registers the types detected from the given collection.
        /// </param>
        public static IServiceCollection AddEventPublishersAndHandlers(this IServiceCollection serviceCollection,
                                                                       IEnumerable<Type> serviceTypes = null)
        {
            serviceCollection.AddSingleton<IBackgroundServiceProvider>(provider => new BackgroundServiceProvider(provider));

            // Event Pub/Sub
            serviceCollection.AddScoped<IEventPublisher, EventPublisher>();

            if (serviceTypes != null)
            {
                serviceCollection.RegisterServiceImplementations<IEventHandler>(serviceTypes,
                                                                                ServiceLifetime.Scoped,
                                                                                true);
            }
            return serviceCollection;
        }

        public static IServiceCollection AddEventHandlersFromTypes(this IServiceCollection serviceCollection,
                                                                   IEnumerable<Type> serviceTypes)
        {
            if (serviceTypes != null)
            {
                serviceCollection.RegisterServiceImplementations<IEventHandler>(serviceTypes,
                                                                                ServiceLifetime.Scoped,
                                                                                true);
            }
            return serviceCollection;
        }

        public static IServiceCollection AddEventHandlersFromAssembly(this IServiceCollection serviceCollection, Assembly assembly)
        {
            serviceCollection.RegisterServiceImplementations<IEventHandler>(new[] { assembly },
                                                                            ServiceLifetime.Scoped,
                                                                            true);

            return serviceCollection;
        }

        public static IServiceCollection AddEventHandlersFromAssemblies(this IServiceCollection serviceCollection, Assembly[] assemblies)
        {
            serviceCollection.RegisterServiceImplementations<IEventHandler>(assemblies,
                                                                            ServiceLifetime.Scoped,
                                                                            true);

            return serviceCollection;
        }
    }
}
