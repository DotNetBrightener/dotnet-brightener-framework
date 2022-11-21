using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionEventPublisherExtensions
{
    /// <summary>
    ///     Registers the event publish / Subscribe service to the <see cref="serviceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    public static IServiceCollection AddEventPubSubService(this IServiceCollection serviceCollection)
    {
        // Event Pub/Sub
        serviceCollection.AddScoped<IEventPublisher, EventPublisher>();

        return serviceCollection;
    }

    /// <summary>
    ///     Detects and registers the event handlers from given list of assemblies
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <param name="assemblies">
    ///     The array of assemblies to detect <see cref="IEventHandler"/> implementations and register them
    /// </param>
    /// <returns></returns>
    public static IServiceCollection AddEventHandlersFromAssemblies(this IServiceCollection serviceCollection,
                                                                    Assembly[]              assemblies)
    {
        serviceCollection.RegisterServiceImplementations<IEventHandler>(assemblies,
                                                                        ServiceLifetime.Scoped,
                                                                        true);

        return serviceCollection;
    }

    /// <summary>
    ///     Registers the provided implementation type <typeparamref name="TEventHandler"/> as <see cref="IEventHandler"/> to the <paramref name="serviceCollection"/>
    /// </summary>
    /// <typeparam name="TEventHandler">The type of the implementation for <see cref="IEventHandler"/></typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <returns>
    ///     The <see cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection AddEventHandler<TEventHandler>(this IServiceCollection serviceCollection)
        where TEventHandler : class, IEventHandler
    {
        serviceCollection.AddScoped<IEventHandler, TEventHandler>();

        return serviceCollection;
    }
}