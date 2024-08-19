using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Auditing.Entities;

/// <summary>
///     Represents the entity for audit trail.
/// </summary>
public class AuditEntity
{
    [Key]
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();

    [MaxLength(256)]
    public string EntityType { get; set; }

    [MaxLength(2048)]
    public string EntityTypeFullName { get; set; }

    [MaxLength(128)]
    public string EntityIdentifier { get; set; }

    [MaxLength(64)]
    public string Action { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime   { get; set; }

    public string Changes { get; set; }

    public string Exception { get; set; }

    public bool IsSuccess { get; set; }

    [MaxLength(255)]
    public string UserName { get; set; }
}