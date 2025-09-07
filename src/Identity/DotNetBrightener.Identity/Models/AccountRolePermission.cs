using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
///     Represents a permission assigned to a role within a specific account
/// </summary>
public class AccountRolePermission : GuidBaseEntityWithAuditInfo
{
    /// <summary>
    ///     Gets or sets the account role ID
    /// </summary>
    public Guid AccountRoleId { get; set; }

    /// <summary>
    ///     Gets or sets the permission ID
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    ///     Gets or sets whether this permission assignment is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Navigation property for the account role
    /// </summary>
    public virtual AccountRole AccountRole { get; set; } = null!;

    /// <summary>
    ///     Navigation property for the permission
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}
