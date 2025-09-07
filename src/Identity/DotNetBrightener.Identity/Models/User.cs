using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
///     Abstract base class representing a user in the identity system
/// </summary>
public abstract class User : GuidBaseEntityWithAuditInfo
{
    /// <summary>
    ///     Gets or sets the user name for this user
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    ///     Gets or sets the normalized user name for this user
    /// </summary>
    public string? NormalizedUserName { get; set; }

    /// <summary>
    ///     Gets or sets the email address for this user
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Gets or sets the normalized email address for this user
    /// </summary>
    public string? NormalizedEmail { get; set; }

    /// <summary>
    ///     Gets or sets a flag indicating if a user has confirmed their email address
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    ///     A random value that must change whenever a user is persisted to the store
    /// </summary>
    public string? ConcurrencyStamp { get; set; }

    /// <summary>
    ///     Gets or sets a telephone number for the user
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets a flag indicating if a user has confirmed their telephone address
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    ///     Gets or sets a flag indicating if two-factor authentication is enabled for this user
    /// </summary>
    public bool MultiFactorAuthEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the date and time, in UTC, when user's password expires
    /// </summary>
    public DateTimeOffset? PasswordExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time, in UTC, when any user lockout ends
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    ///     Gets or sets a flag indicating if the user could be locked out
    /// </summary>
    public bool LockoutEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the number of failed login attempts for the current user
    /// </summary>
    public int AccessFailedCount { get; set; }

    /// <summary>
    ///     Gets or sets the user's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    ///     Gets or sets the user's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    ///     Gets or sets the user's full display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets whether the user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets the user's time zone
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    ///     Gets or sets the user's preferred language/culture
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    ///     Navigation property for user's account memberships
    /// </summary>
    public virtual ICollection<UserAccountMembership> AccountMemberships { get; set; }

    /// <summary>
    ///     Computed property for full name
    /// </summary>
    public string FullName => !string.IsNullOrEmpty(DisplayName)
        ? DisplayName
        : $"{FirstName} {LastName}".Trim();
}