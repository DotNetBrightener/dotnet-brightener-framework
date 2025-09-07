using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
/// Represents password-related information for a user, stored separately for security
/// </summary>
public class UserPassword : GuidBaseEntityWithAuditInfo
{
    public UserPassword()
    {
        IsActive = true;
        PasswordChangeHistory = new List<UserPasswordHistory>();
    }

    /// <summary>
    ///     Gets or sets the user ID this password belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets a salted and hashed representation of the password for this user
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    ///     A random value that must change whenever a users credentials change (password changed, login removed)
    /// </summary>
    public string SecurityStamp { get; set; } = string.Empty;

    /// <summary>
    ///     A random value that must change whenever a user is persisted to the store
    /// </summary>
    public string? ConcurrencyStamp { get; set; }

    /// <summary>
    ///     Gets or sets whether this password record is active (current)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets when this password was set
    /// </summary>
    public DateTimeOffset PasswordSetAt { get; set; }

    /// <summary>
    ///     Gets or sets when this password expires (null for no expiration)
    /// </summary>
    public DateTimeOffset? PasswordExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets whether the user must change their password on next login
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    ///     Gets or sets the reason for the password change
    /// </summary>
    public string? PasswordChangeReason { get; set; }

    /// <summary>
    ///     Gets or sets the IP address from which the password was changed
    /// </summary>
    public string? PasswordChangeIpAddress { get; set; }

    /// <summary>
    ///     Gets or sets the user agent from which the password was changed
    /// </summary>
    public string? PasswordChangeUserAgent { get; set; }

    /// <summary>
    ///     Navigation property for the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    ///     Navigation property for password change history
    /// </summary>
    public virtual ICollection<UserPasswordHistory> PasswordChangeHistory { get; set; }

    /// <summary>
    ///     Checks if the password is currently valid and not expired
    /// </summary>
    public bool IsPasswordValid()
    {
        return IsActive && 
               !IsDeleted &&
               (PasswordExpiresAt == null || PasswordExpiresAt > DateTimeOffset.UtcNow);
    }

    /// <summary>
    ///     Checks if the password needs to be changed
    /// </summary>
    public bool RequiresPasswordChange()
    {
        return MustChangePassword || 
               (PasswordExpiresAt != null && PasswordExpiresAt <= DateTimeOffset.UtcNow);
    }
}
