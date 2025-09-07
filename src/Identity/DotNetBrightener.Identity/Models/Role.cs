using System.ComponentModel.DataAnnotations;
using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
/// Abstract base class representing a global role that can be used across accounts
/// </summary>
public abstract class Role : GuidBaseEntityWithAuditInfo
{
    protected Role()
    {
        
    }

    protected Role(string code, string roleName)
    {
        RoleCode = code;
        Name     = roleName;
    }

    protected Role(string roleName)
    {
        Name     = roleName;
        RoleCode = roleName.Replace(" ", "")
                           .Trim();
    }

    /// <summary>
    ///     Gets or sets the unique code for the role
    /// </summary>
    [MaxLength(64)]
    public string RoleCode { get; set; }


    /// <summary>
    ///     Gets or sets the name for this role
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets whether the role is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets whether this is a system role that cannot be deleted
    /// </summary>
    public bool IsSystemRole { get; set; }
}