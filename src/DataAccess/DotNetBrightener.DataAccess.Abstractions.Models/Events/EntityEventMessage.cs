#nullable enable
using DotNetBrightener.DataAccess.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Events;

public abstract class EntityEventMessage<TEntity> : IEventMessage where TEntity : class
{
    public long EntityId { get; set; }

    public TEntity? Entity { get; set; }

    public AuditTrail<TEntity>? AuditTrail { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }
}

/// <summary>
///     Event fired when an entity of type <typeparam name="TEntity" /> is created
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityCreated<TEntity>: EntityEventMessage<TEntity> where TEntity : class;


/// <summary>
///     Event fired when an entity of type <typeparam name="TEntity" /> is updated
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityUpdated<TEntity>: EntityEventMessage<TEntity> where TEntity : class;


/// <summary>
///     Event fired when an entity of type <typeparam name="TEntity" /> is deleted
/// </summary>
/// <typeparam name="TEntity">
///     The type of the entity specified in the event
/// </typeparam>
public class EntityDeleted<TEntity>: EntityEventMessage<TEntity> where TEntity : class;


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