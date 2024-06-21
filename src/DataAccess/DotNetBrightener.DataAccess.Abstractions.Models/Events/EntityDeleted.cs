#nullable enable
// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

/// <summary>
///     Event fired when an entity of type <typeparam name="TEntity" /> is deleted
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityDeleted<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public EntityDeleted()
    {
    }

    public EntityDeleted(TEntity entity, string? userId, string? userName)
        : base(entity, userId, userName)
    {
    }
}