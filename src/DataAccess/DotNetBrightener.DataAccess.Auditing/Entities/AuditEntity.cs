using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Auditing.Entities;

/// <summary>
///     Represents the entity for audit trail.
/// </summary>
public class AuditEntity
{
    [Key]
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();

    [MaxLength(64)]
    public string Action { get; set; }

    public string Changes { get; set; }

    [MaxLength(256)]
    public string EntityType { get; set; }

    [MaxLength(64)]
    public string EntityIdentifier { get; set; }

    [MaxLength(2048)]
    public string EntityTypeFullName { get; set; }

    public bool IsSuccess { get; set; }

    public string Exception { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    [MaxLength(255)]
    public string UserName { get; set; }

    [MaxLength(2048)]
    public string Url { get; set; }

    public TimeSpan? Duration { get; set; }

    public Guid ScopeId { get; set; } = Ulid.NewUlid().ToGuid();
}