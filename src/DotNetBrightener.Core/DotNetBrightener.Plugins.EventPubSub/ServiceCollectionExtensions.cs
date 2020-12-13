using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.Plugins.EventPubSub.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.Extensions
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
        ///     Otherwise, detects and registers from all assemblies loaded into the application.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddEventPublishersAndHandlers(this IServiceCollection serviceCollection,
                                                                       IEnumerable<Type> serviceTypes = null)
        {
            serviceCollection.AddSingleton<IBackgroundServiceProvider>(provider => new BackgroundServiceProvider(provider));

            // Event Pub/Sub
            serviceCollection.AddScoped<IEventPublisher, EventPublisher>();

            if (serviceTypes == null)
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                serviceCollection.RegisterServiceImplementations<IEventHandler>(appAssemblies,
                                                                                ServiceLifetime.Scoped);
            }
            else
            {
                serviceCollection.RegisterServiceImplementations<IEventHandler>(serviceTypes,
                                                                                ServiceLifetime.Scoped);
            }
            return serviceCollection;
        }
    }
}
