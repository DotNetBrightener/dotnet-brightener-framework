#nullable enable
using System.Linq.Expressions;

// ReSharper disable CheckNamespace

namespace DotNetBrightener.DataAccess.Events;

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

    public int AffectedRecords { get; set; }

    public string? DeletionReason { get; set; }
}