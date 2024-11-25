#nullable enable

// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

/// <summary>
///     Event fired before an update to entity of type <typeparamref name="TEntity" /> is persisted
/// </summary>
/// <remarks>
///     The <seealso cref="EntityEventMessage{TEntity}.Entity"/> contains the data of the entity that are being persisted.
///     Changes made to this entity will be persisted.
/// </remarks>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityUpdating<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public EntityUpdating()
    {

    }

    public EntityUpdating(TEntity entity, string? userId, string? userName)
        : base(entity, userId, userName)
    {

    }
}