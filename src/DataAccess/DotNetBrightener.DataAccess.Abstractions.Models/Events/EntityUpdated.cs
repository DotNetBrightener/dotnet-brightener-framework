#nullable enable

// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

/// <summary>
///     Event fired when an entity of type <typeparam name="TEntity" /> is updated
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityUpdated<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public EntityUpdated()
    {
    }

    public EntityUpdated(TEntity entity, string? userId, string? userName)
        : base(entity, userId, userName)
    {
    }
}