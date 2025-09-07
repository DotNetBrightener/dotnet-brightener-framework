using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
///     Represents a permission assigned to an account
/// </summary>
public class AccountPermission : GuidBaseEntityWithAuditInfo
{
    public AccountPermission()
    {
        IsActive = true;
    }

    /// <summary>
    ///     Gets or sets the account ID
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Gets or sets the permission ID
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    ///     Gets or sets whether this permission assignment is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Navigation property for the account
    /// </summary>
    public virtual Account Account { get; set; } = null!;

    /// <summary>
    ///     Navigation property for the permission
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}
