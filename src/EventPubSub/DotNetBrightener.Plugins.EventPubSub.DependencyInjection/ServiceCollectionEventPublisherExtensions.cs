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
    public static EventPubSubServiceBuilder AddEventPubSubService(this   IServiceCollection serviceCollection,
                                                                  params Assembly[]         assembliesContainMessages)
    {
        var appAssemblies = assembliesContainMessages.Length == 0
                                ? AppDomain.CurrentDomain.GetAppOnlyAssemblies()
                                : assembliesContainMessages;

        var eventHandlerTypes = appAssemblies.GetDerivedTypes<IEventMessage>();

        // Event Pub/Sub
        serviceCollection.TryAddScoped<IEventPublisher, DefaultEventPublisher>();

        var eventPubSubBuilder = new EventPubSubServiceBuilder
        {
            Services          = serviceCollection,
            EventMessageTypes = eventHandlerTypes
        };

        serviceCollection.AddSingleton(eventPubSubBuilder);

        return eventPubSubBuilder;
    }

    /// <summary>
    ///     Detects and registers the event handlers from given list of assemblies
    /// </summary>
    /// <param name="serviceBuilder">
    ///     The <see cref="EventPubSubServiceBuilder"/>
    /// </param>
    /// <param name="assemblies">
    ///     The array of assemblies to detect <see cref="IEventHandler"/> implementations and register them
    /// </param>
    /// <returns></returns>
    public static EventPubSubServiceBuilder AddEventHandlersFromAssemblies(
        this EventPubSubServiceBuilder serviceBuilder,
        Assembly[]                     assemblies)
    {
        serviceBuilder.Services.RegisterServiceImplementations<IEventHandler>(assemblies,
                                                                              ServiceLifetime.Scoped,
                                                                              true);

        return serviceBuilder;
    }

    /// <summary>
    ///     Registers the provided implementation type <typeparamref name="TEventHandler"/> as <see cref="IEventHandler"/> to the <paramref name="serviceBuilder"/>
    /// </summary>
    /// <typeparam name="TEventHandler">The type of the implementation for <see cref="IEventHandler"/></typeparam>
    /// <param name="serviceBuilder">
    ///     The <see cref="EventPubSubServiceBuilder"/>
    /// </param>
    /// <returns>
    ///     The <see cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static EventPubSubServiceBuilder AddEventHandler<TEventHandler>(
        this EventPubSubServiceBuilder serviceBuilder)
        where TEventHandler : class, IEventHandler
    {
        serviceBuilder.Services.AddScoped<IEventHandler, TEventHandler>();

        return serviceBuilder;
    }

    internal static Assembly[] GetAppOnlyAssemblies(this AppDomain appDomain)
    {
        return appDomain.GetAssemblies()
                        .FilterSkippedAssemblies()
                        .ToArray();
    }
}