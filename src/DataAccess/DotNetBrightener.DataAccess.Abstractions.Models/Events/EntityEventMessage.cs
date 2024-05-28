#nullable enable
using DotNetBrightener.DataAccess.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Events;

public abstract class EntityEventMessage<TEntity> : IEventMessage where TEntity : class
{
    protected EntityEventMessage()
    {
        
    }

    protected EntityEventMessage(TEntity entity, string? userId, string? userName)
    {
        Entity   = entity;
        UserId   = userId;
        UserName = userName;
    }

    /// <summary>
    ///     The identifier of the entity associated with this event message
    /// </summary>
    public long? EntityId { get; set; }

    /// <summary>
    ///     The entity of the event message
    /// </summary>
    public TEntity? Entity { get; set; }

    public AuditTrail<TEntity>? AuditTrail { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }
}

/// <summary>
///     Event fired when an entity of type <typeparamref name="TEntity" /> is created
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


/// <summary>
///     Event fired before the entity of type <typeparam name="TEntity" /> is created and persisted
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

    public EntityCreating(TEntity entity, string? userId, string? userName) : base(entity, userId, userName)
    {
        
    }
}


/// <summary>
///     Event fired before an update to entity of type <typeparam name="TEntity" /> is persisted
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
    public object DataToUpdate { get; set; }

    public EntityUpdating()
    {
        
    }

    public EntityUpdating(TEntity entity, string? userId, string? userName) : base(entity, userId, userName)
    {
        
    }
}


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

    public EntityUpdated(TEntity entity, string? userId, string? userName) : base(entity, userId, userName)
    {
        
    }
}


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

    public EntityDeleted(TEntity entity, string? userId, string? userName) : base(entity, userId, userName)
    {
        
    }
}


/// <summary>
///     Event fired when entities of type <typeparam name="TEntity" /> are updated using expression
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entities specified in the event
/// </typeparam>
public class EntityUpdatedByExpression<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>>?    FilterExpression { get; set; }

    public Expression<Func<TEntity, TEntity>>? UpdateExpression { get; set; }

    public int AffectedRecords { get; set; }
}


/// <summary>
///     Event fired when entities of type <typeparam name="TEntity" /> are deleted using expression
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entities specified in the event
/// </typeparam>
public class EntityDeletedByExpression<TEntity> : EntityEventMessage<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>>? FilterExpression { get; set; }

    public bool IsHardDeleted { get; set; }

    public int     AffectedRecords { get; set; }

    public string? DeletionReason  { get; set; }
}