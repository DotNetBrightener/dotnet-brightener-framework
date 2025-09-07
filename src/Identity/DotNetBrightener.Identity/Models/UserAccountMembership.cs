using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
/// Represents a user's membership in an account with specific roles
/// </summary>
public class UserAccountMembership : GuidBaseEntityWithAuditInfo
{
    public UserAccountMembership()
    {
        IsActive = true;
        UserRoles = new List<UserAccountRole>();
    }

    /// <summary>
    ///     Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the account ID
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Gets or sets whether this membership is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets when the membership expires (null for no expiration)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets whether the user can access sub-accounts
    /// </summary>
    public bool CanAccessSubAccounts { get; set; }

    /// <summary>
    ///     Gets or sets additional membership metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    ///     Navigation property for the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    ///     Navigation property for the account
    /// </summary>
    public virtual Account Account { get; set; } = null!;

    /// <summary>
    ///     Navigation property for the user's roles in this account
    /// </summary>
    public virtual ICollection<UserAccountRole> UserRoles { get; set; }

    /// <summary>
    ///     Checks if the membership is currently valid
    /// </summary>
    public bool IsValid()
    {
        return IsActive && 
               (ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow);
    }
}
