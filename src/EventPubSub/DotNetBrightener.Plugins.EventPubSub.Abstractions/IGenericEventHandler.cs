using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace DotNetBrightener.Plugins.EventPubSub;

internal class GenericEventHandlersContainer
{
    public List<Type> GenericEventHandlerTypes { get; init; } = new();

    public ConcurrentDictionary<Type, List<Type>> MappedMessageTypeAndHandlers { get; } = new();
}

internal interface IGenericEventHandler : IEventHandler
{
    Task<bool> HandleEvent<T>(T eventMessage) where T : IEventMessage;
}

internal class GenericEventHandler : IGenericEventHandler
{
    private readonly IServiceProvider              _serviceProvider;
    private readonly GenericEventHandlersContainer _container;

    public GenericEventHandler(IServiceProvider              serviceProvider,
                               IServiceCollection            services,
                               GenericEventHandlersContainer container)
    {
        _serviceProvider = serviceProvider;
        _container       = container;

        if (container.GenericEventHandlerTypes.Count == 0)
        {
            container.GenericEventHandlerTypes.AddRange([
                .. services.Where(x => x.ImplementationType is not null &&
                                       x.ImplementationType.IsGenericType &&
                                       x.ImplementationType
                                        .IsAssignableTo(typeof(IEventHandler)))
                           .Select(x => x.ImplementationType)
                           .Distinct()
            ]);
        }
    }

    public int Priority => 10_000;

    public async Task<bool> HandleEvent<T>(T eventMessage) where T : IEventMessage
    {
        var messageType = eventMessage.GetType();

        if (!typeof(T).IsGenericType)
        {
            // this handler is specific for handling generic event messages, just short-circuit if not
            return false;
        }

        
        var genericTypeDef = typeof(T).GetGenericTypeDefinition();
        var genericTypeArgs = typeof(T).GetGenericArguments();
        
        if (genericTypeArgs.Length != 1)
        {
            // this handler is specific for handling generic event messages with one type argument, just short-circuit if not
            return false;
        }

        var msgGenericTypeArg = genericTypeArgs[0];

        if (!_container.MappedMessageTypeAndHandlers
                       .TryGetValue(genericTypeDef, out var foundEventHandlerTypes))
        {
            foundEventHandlerTypes = new List<Type>();

            foreach (var t in _container.GenericEventHandlerTypes)
            {
                var implementedInterface = t.GetInterfaces()
                                            .FirstOrDefault(x => x.IsGenericType);

                if (implementedInterface is null)
                    continue;

                var typeDef = implementedInterface.GetGenericArguments()
                                                  .FirstOrDefault();

                if (typeDef is null ||
                    !typeDef.IsGenericType)
                    continue;

                typeDef = typeDef.GetGenericTypeDefinition();

                if (genericTypeDef == typeDef)
                {
                    foundEventHandlerTypes.Add(t);
                }
            }

            if (foundEventHandlerTypes.Count > 0)
                _container.MappedMessageTypeAndHandlers.TryAdd(genericTypeDef, foundEventHandlerTypes);
        }

        var expectingEventHandlerType = typeof(IEventHandler<>).MakeGenericType(messageType);

        if (!_container.MappedMessageTypeAndHandlers
                       .TryGetValue(expectingEventHandlerType, out var otherHandlers))
        {
            otherHandlers = new List<Type>();
            var allEventHandlers = _serviceProvider.GetServices<IEventHandler>();

            foreach (var eventHandler in allEventHandlers)
            {
                var handlerType = eventHandler.GetType();

                if (handlerType.IsAssignableTo(expectingEventHandlerType))
                {
                    otherHandlers.Add(handlerType);
                }
            }

            if (otherHandlers.Count > 0)
                _container.MappedMessageTypeAndHandlers.TryAdd(expectingEventHandlerType, otherHandlers);
        }

        var eventHandlers = foundEventHandlerTypes
                           .Concat(otherHandlers)
                           .Select(x =>
                            {
                                var finalHandlerType = x.IsGenericType ? x.MakeGenericType(msgGenericTypeArg) : x;

                                var eventHandler = _serviceProvider.GetService(finalHandlerType);

                                return eventHandler;
                            })
                           .Where(x => x is not null)
                           .OfType<IEventHandler>()
                           .OrderByDescending(x => x.Priority)
                           .ToList();

        var tasks = new List<Task>();

        foreach (var eventHandler in eventHandlers)
        {
            var handleEventMethod = eventHandler.GetMethodWithName(nameof(IEventHandler<T>.HandleEvent), typeof(T));


            if (handleEventMethod == null)
                continue;

            if (handleEventMethod.Invoke(eventHandler,
                [
                    eventMessage
                ]) is Task<bool> result)
            {
                tasks.Add(result);

                if (eventMessage is INonStoppedEventMessage)
                {
                    continue;
                }

                var finalResult = await result;

                if (!finalResult)
                {
                    return false;
                }
            }
        }

        if (eventMessage is INonStoppedEventMessage)
        {
            await Task.WhenAll(tasks);
        }

        return true;
    }
}