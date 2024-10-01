using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.DataAccess.Models.Utils.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.DataAccess.Auditing.Entities;

/// <summary>
///     Represents the entity for audit trail.
/// </summary>
public class AuditEntity
{
    public Guid Id { get; set; } = Uuid7.Guid();

    [MaxLength(64)]
    public string Action { get; set; }

    public string Changes { get; set; }

    [MaxLength(256)]
    public string EntityType { get; set; }

    [MaxLength(64)]
    public string EntityIdentifier { get; set; }

    [MaxLength(2048)]
    public string EntityTypeFullName { get; set; }

    public string DebugView { get; set; }

    public bool IsSuccess { get; set; }

    public string Exception { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    [MaxLength(255)]
    public string UserName { get; set; }

    [MaxLength(2048)]
    public string Url { get; set; }

    public TimeSpan? Duration { get; set; }

    public Guid ScopeId { get; set; } = Uuid7.Guid();

    [NotMapped]
    public EntityEntry AssociatedEntityEntry { get; internal init; }

    [NotMapped]
    public ImmutableList<AuditProperty> AuditProperties { get; internal init; } = ImmutableList<AuditProperty>.Empty;

    [NotMapped]
    public ImmutableList<AuditProperty> ChangedAuditProperties => Action == EntityState.Added.ToString() ||
                                                                  Action == EntityState.Deleted.ToString()
                                                                      ? AuditProperties
                                                                      : AuditProperties
                                                                       .Where(x => !x.OldValue.Equals(x.NewValue))
                                                                       .ToImmutableList();
}