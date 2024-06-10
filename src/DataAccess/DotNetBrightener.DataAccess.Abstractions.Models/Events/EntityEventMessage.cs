#nullable enable
using DotNetBrightener.DataAccess.Auditing;
using DotNetBrightener.Plugins.EventPubSub;

// ReSharper disable CheckNamespace

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