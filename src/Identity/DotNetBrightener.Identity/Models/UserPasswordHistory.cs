using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
/// Represents historical password information for tracking password changes
/// </summary>
public class UserPasswordHistory : GuidBaseEntityWithAuditInfo
{
    public UserPasswordHistory()
    {
        
    }

    /// <summary>
    ///     Gets or sets the user password ID this history entry belongs to
    /// </summary>
    public Guid UserPasswordId { get; set; }

    /// <summary>
    ///     Gets or sets the user ID for quick reference
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the previous password hash (for preventing password reuse)
    /// </summary>
    public string? PreviousPasswordHash { get; set; }

    /// <summary>
    ///     Gets or sets the previous security stamp
    /// </summary>
    public string? PreviousSecurityStamp { get; set; }

    /// <summary>
    ///     Gets or sets when the password was changed
    /// </summary>
    public DateTimeOffset PasswordChangedAt { get; set; }

    /// <summary>
    ///     Gets or sets the reason for the password change
    /// </summary>
    public string? ChangeReason { get; set; }

    /// <summary>
    ///     Gets or sets the IP address from which the password was changed
    /// </summary>
    public string? ChangeIpAddress { get; set; }

    /// <summary>
    ///     Gets or sets the user agent from which the password was changed
    /// </summary>
    public string? ChangeUserAgent { get; set; }

    /// <summary>
    ///     Gets or sets whether this was a forced password change
    /// </summary>
    public bool WasForcedChange { get; set; }

    /// <summary>
    ///     Gets or sets whether this change was due to a security incident
    /// </summary>
    public bool WasSecurityIncident { get; set; }

    /// <summary>
    ///     Navigation property for the user password
    /// </summary>
    public virtual UserPassword UserPassword { get; set; } = null!;

    /// <summary>
    ///     Navigation property for the user
    /// </summary>
    public virtual User User { get; set; } = null!;
}
