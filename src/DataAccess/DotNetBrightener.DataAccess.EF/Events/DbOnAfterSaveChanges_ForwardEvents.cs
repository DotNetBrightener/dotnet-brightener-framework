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


        var tasksList = new List<IEventMessage>();

        ProcessEntitiesEvent(eventMessage.InsertedEntityEntries, tasksList, typeof(EntityCreated<>));

        ProcessEntitiesUpdatedEvent(eventMessage.UpdatedEntityEntries, tasksList, typeof(EntityUpdated<>));

        if (tasksList.Any())
        {
            await tasksList.ParallelForEachAsync(eventMsg => _eventPublisher.Publish(eventMsg));
        }

        return true;
    }

    public int Priority => 10_000;


    private void ProcessEntitiesEvent(EntityEntry[] entityEntries, List<IEventMessage> tasksList, Type eventType)
    {
        var entityTypes = entityEntries.Select(entry => entry.Entity.GetType())
                                       .Distinct()
                                       .ToArray();

        foreach (var entityType in entityTypes)
        {
            var eventMessageType = eventType.MakeGenericType(entityType);

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
                    tasksList.Add(entityEventMsg);
                }
            }
        }
    }


    private void ProcessEntitiesUpdatedEvent(EntityEntry[] entityEntries, List<IEventMessage> tasksList, Type eventType)
    {
        var entityTypes = entityEntries.Select(entry => entry.Entity.GetType())
                                       .Distinct()
                                       .ToArray();

        foreach (var entityType in entityTypes)
        {
            var eventMessageType = eventType.MakeGenericType(entityType);

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
                    tasksList.Add(entityEventMsg);
                }
            }
        }
    }
}