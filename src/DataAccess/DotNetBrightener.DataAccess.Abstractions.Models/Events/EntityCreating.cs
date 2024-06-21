#nullable enable

// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

/// <summary>
///     Event fired before the entity of type <typeparam name="TEntity" /> is preparing, before it's persisted to the data store
/// </summary>
/// <remarks>
///     The <seealso cref="EntityEventMessage{TEntity}.Entity"/> contains the data of the entity that are being persisted.
///     Changes made to this entity will be persisted.
/// </remarks>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityCreating<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public EntityCreating()
    {

    }

    public EntityCreating(TEntity entity, string? userId, string? userName)
        : base(entity, userId, userName)
    {

    }
}