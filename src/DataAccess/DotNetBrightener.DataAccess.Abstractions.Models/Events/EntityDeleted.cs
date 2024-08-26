#nullable enable
// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

/// <summary>
///     Event fired when an entity of type <typeparamref name="TEntity" /> is deleted
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

/// <summary>
///     Event fired when an entity of type <typeparamref name="TEntity" /> is being deleted
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityDeleting<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public string? DeletionReason { get; set; }

    public EntityDeleting()
    {
    }

    public EntityDeleting(TEntity entity, string? userId, string? userName)
        : base(entity, userId, userName)
    {
    }
}