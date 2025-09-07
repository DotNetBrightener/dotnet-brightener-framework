using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
/// Represents a user's assignment to a role within a specific account
/// </summary>
public class UserAccountRole : GuidBaseEntityWithAuditInfo
{
    public UserAccountRole()
    {
        IsActive = true;
    }

    /// <summary>
    ///     Gets or sets the user account membership ID
    /// </summary>
    public Guid UserAccountMembershipId { get; set; }

    /// <summary>
    ///     Gets or sets the account role ID
    /// </summary>
    public Guid AccountRoleId { get; set; }

    /// <summary>
    ///     Gets or sets whether this role assignment is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets when the role assignment expires (null for no expiration)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     Navigation property for the user account membership
    /// </summary>
    public virtual UserAccountMembership UserAccountMembership { get; set; } = null!;

    /// <summary>
    ///     Navigation property for the account role
    /// </summary>
    public virtual AccountRole AccountRole { get; set; } = null!;

    /// <summary>
    ///     Checks if the role assignment is currently valid
    /// </summary>
    public bool IsValid()
    {
        return IsActive && 
               (ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow);
    }
}
