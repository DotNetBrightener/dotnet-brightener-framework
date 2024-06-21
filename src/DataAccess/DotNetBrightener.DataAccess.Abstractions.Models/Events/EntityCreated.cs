#nullable enable

// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

/// <summary>
///     Event fired when an entity of type <typeparamref name="TEntity" /> is created, after it's persisted to the data store
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityCreated<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public EntityCreated(): base()
    {
        
    }

    public EntityCreated(TEntity entity, string? userId, string? userName): base(entity, userId, userName)
    {
    }
}