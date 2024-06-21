#nullable enable
using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DotNetBrightener.DataAccess.EF.Events;

public class DbOnAfterSaveChanges_ForwardEvents(IServiceProvider serviceProvider)
    : IEventHandler<DbContextAfterSaveChanges>
{
    private readonly IEventPublisher? _eventPublisher = serviceProvider.TryGet<IEventPublisher>();

    private readonly ICurrentLoggedInUserResolver? _currentLoggedInUserResolver =
        serviceProvider.TryGet<ICurrentLoggedInUserResolver>();

    public async Task<bool> HandleEvent(DbContextAfterSaveChanges eventMessage)
    {
        if (_eventPublisher is null)
            return true;


        var eventMessages = new List<IEventMessage>();

        ProcessEntitiesEvent(eventMessage.InsertedEntityEntries, eventMessages, typeof(EntityCreated<>));

        ProcessEntitiesEvent(eventMessage.UpdatedEntityEntries, eventMessages, typeof(EntityUpdated<>));

        if (eventMessages.Any())
        {
            await eventMessages.ParallelForEachAsync(eventMsg => _eventPublisher.Publish(eventMsg));
        }

        return true;
    }

    public int Priority => 10_000;

    private const string CastleProxies = "Castle.Proxies.";


    private void ProcessEntitiesEvent(List<EntityEntry>   entityEntries,
                                      List<IEventMessage> eventMessages,
                                      Type                eventType)
    {
        if (!entityEntries.Any())
        {
            return;
        }

        var entityTypes = entityEntries.Select(entry => entry.Entity.GetType())
                                       .Distinct()
                                       .ToArray();

        foreach (var entityType in entityTypes)
        {
            var actualEntityType = entityType.FullName?.StartsWith(CastleProxies) == true
                                       ? entityType.BaseType
                                       : entityType;

            var eventMessageType = eventType.MakeGenericType(actualEntityType!);

            var entries = entityEntries.Where(entry => entry.Entity.GetType() == entityType)
                                       .ToArray();

            foreach (var record in entries)
            {
                if (Activator.CreateInstance(eventMessageType,
                                             record.Entity,
                                             _currentLoggedInUserResolver?.CurrentUserId,
                                             _currentLoggedInUserResolver?.CurrentUserName) is IEventMessage
                    entityEventMsg)
                {
                    eventMessages.Add(entityEventMsg);
                }
            }
        }
    }
}