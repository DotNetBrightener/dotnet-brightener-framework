using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
///     Represents a permission assigned directly to a user
/// </summary>
public class UserPermission : GuidBaseEntityWithAuditInfo
{
    public UserPermission()
    {
        IsActive = true;
    }

    /// <summary>
    ///     Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the permission ID
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    ///     Gets or sets the account ID this permission is scoped to (null for global)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    ///     Gets or sets whether this permission assignment is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets when the permission assignment expires (null for no expiration)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     Navigation property for the permission
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}
