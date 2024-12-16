using DotNetBrightener.DataAccess.Models.Auditing;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.DataAccess.EF.Auditing;

/// <summary>
///     Represents the entity for audit trail.
/// </summary>
public class AuditEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [MaxLength(64)]
    public string Action { get; set; }

    public string Changes { get; set; }

    [MaxLength(256)]
    public string EntityType { get; set; }

    public string EntityIdentifier { get; set; }
    
    public string DebugView { get; set; }

    public bool IsSuccess { get; set; }

    public int? AffectedRows { get; set; }

    public string Exception { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    [MaxLength(255)]
    public string UserName { get; set; }

    [MaxLength(2048)]
    public string Url { get; set; }

    public TimeSpan? Duration { get; set; }

    public Guid ScopeId { get; set; } = Guid.CreateVersion7();

    [MaxLength(64)]
    public string AuditToolVersion { get; set; }

    [NotMapped]
    public EntityEntry AssociatedEntityEntry { get; internal init; }

    [NotMapped]
    public ImmutableList<AuditProperty> AuditProperties { get; internal set; } = ImmutableList<AuditProperty>.Empty;
}