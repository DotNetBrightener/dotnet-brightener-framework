using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
///     Abstract base class representing a tenant/account in the multi-tenant system
/// </summary>
public abstract class Account : GuidBaseEntityWithAuditInfo
{
    /// <summary>
    ///     Gets or sets the account name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the account description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the parent account ID for hierarchical accounts
    /// </summary>
    public Guid? ParentAccountId { get; set; }

    /// <summary>
    ///     Gets or sets whether the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets the account settings as JSON
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    ///     Navigation property for the parent account
    /// </summary>
    [ForeignKey(nameof(ParentAccountId))]
    public virtual Account? ParentAccount { get; set; }
}
