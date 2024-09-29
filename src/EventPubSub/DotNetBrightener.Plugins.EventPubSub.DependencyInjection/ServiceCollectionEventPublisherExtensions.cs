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
    /// <returns>
    ///     The <see cref="EventPubSubServiceBuilder"/> for chaining operations
    /// </returns>
    public static EventPubSubServiceBuilder AddEventPubSubService(this   IServiceCollection serviceCollection,
                                                                  params Assembly[]         assembliesContainMessages)
    {
        var appAssemblies = assembliesContainMessages.Length == 0
                                ? AppDomain.CurrentDomain.GetAppOnlyAssemblies()
                                : assembliesContainMessages;

        var eventMessageTypes = appAssemblies.GetDerivedTypes<IEventMessage>()
                                             .Distinct();

        // Event Pub/Sub
        serviceCollection.TryAddScoped<IEventPublisher, DefaultEventPublisher>();

        var eventPubSubBuilder = new EventPubSubServiceBuilder
        {
            Services = serviceCollection
        };

        eventPubSubBuilder.EventMessageTypes.AddRange(eventMessageTypes);

        serviceCollection.AddScoped(typeof(InMemoryEventProcessor<>));

        serviceCollection.AddSingleton(eventPubSubBuilder);
        serviceCollection.AddSingleton<GenericEventHandlersContainer>();
        serviceCollection.AddScoped<IGenericEventHandler, GenericEventHandler>();

        serviceCollection.AddSingleton<InMemoryEventMessageQueue>();
        serviceCollection.AddSingleton<InMemoryEventProcessor>();

        serviceCollection.AddHostedService<InMemoryEventProcessorBackgroundJob>();

        serviceCollection.EnableLazyResolver();

        return eventPubSubBuilder;
    }

    public static EventPubSubServiceBuilder AddEventMessagesFromAssemblies(
        this   EventPubSubServiceBuilder eventPubSubBuilder,
        params Assembly[]                assemblies)
    {
        var eventMessageTypes = assemblies.GetDerivedTypes<IEventMessage>()
                                          .Except(eventPubSubBuilder.EventMessageTypes);

        eventPubSubBuilder.EventMessageTypes.AddRange(eventMessageTypes);

        return eventPubSubBuilder;
    }

    public static IServiceCollection AddEventMessagesFromAssemblies(this   IServiceCollection serviceCollection,
                                                                    params Assembly[]         assemblies)
    {
        if (serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(EventPubSubServiceBuilder) &&
                                                  d.ImplementationInstance is not null)
                            ?.ImplementationInstance is not EventPubSubServiceBuilder eventPubSubBuilder)
        {
            eventPubSubBuilder = new EventPubSubServiceBuilder
            {
                Services = serviceCollection
            };

            serviceCollection.AddSingleton(eventPubSubBuilder);
        }

        eventPubSubBuilder.AddEventMessagesFromAssemblies(assemblies);

        return serviceCollection;
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
    /// <returns>
    ///     The <see cref="EventPubSubServiceBuilder"/> for chaining operations
    /// </returns>
    public static EventPubSubServiceBuilder AddEventHandlersFromAssemblies(
        this   EventPubSubServiceBuilder serviceBuilder,
        params Assembly[]                assemblies)
    {
        serviceBuilder.Services.AddEventHandlersFromAssemblies(assemblies);

        return serviceBuilder;
    }

    /// <summary>
    ///     Detects and registers the event handlers from given list of assemblies
    /// </summary>
    /// <param name="serviceBuilder">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <param name="assemblies">
    ///     The array of assemblies to detect <see cref="IEventHandler"/> implementations and register them
    /// </param>
    /// <returns>
    ///     The <see cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection AddEventHandlersFromAssemblies(this   IServiceCollection serviceBuilder,
                                                                    params Assembly[]         assemblies)
    {
        var appAssemblies = assemblies.Length == 0
                                ? AppDomain.CurrentDomain.GetAppOnlyAssemblies()
                                : assemblies;

        serviceBuilder.RegisterServiceImplementations<IEventHandler>(appAssemblies,
                                                                     ServiceLifetime.Scoped,
                                                                     true);

        return serviceBuilder;
    }

    internal static Assembly[] GetAppOnlyAssemblies(this AppDomain appDomain)
    {
        return appDomain.GetAssemblies()
                        .FilterSkippedAssemblies()
                        .ToArray();
    }
}