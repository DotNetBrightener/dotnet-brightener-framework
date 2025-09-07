using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
///     Represents a role instance within a specific account
/// </summary>
public class AccountRole : GuidBaseEntityWithAuditInfo
{
    /// <summary>
    ///     Gets or sets the account ID this role belongs to
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Gets or sets the global role ID this is based on
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    ///     Gets or sets whether this role instance is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets account-specific role customizations as JSON
    /// </summary>
    public string? CustomSettings { get; set; }
}
