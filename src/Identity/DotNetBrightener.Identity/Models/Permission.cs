using System.Security.Claims;
using DotNetBrightener.Identity.Models.Base;

namespace DotNetBrightener.Identity.Models;

/// <summary>
/// Represents a permission that can be assigned to roles or users
/// </summary>
public class Permission : GuidBaseEntity
{
    /// <summary>
    ///     The identifier for the Claim that represents the permission claim
    /// </summary>
    public const string ClaimType = "Permission";

    public Permission(string permissionKey, string? description = null, string? permissionGroup = null)
    {
        if (string.IsNullOrEmpty(permissionKey))
        {
            throw new ArgumentNullException(nameof(permissionKey));
        }

        PermissionKey = permissionKey;
        Description = description;
        PermissionGroup = permissionGroup;
    }



    /// <summary>
    ///     Gets or sets the unique key of the permission
    /// </summary>
    public string PermissionKey { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the permission
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the description of the permission
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the group/category this permission belongs to
    /// </summary>
    public string? PermissionGroup { get; set; }

    /// <summary>
    ///     Creates a claim from this permission
    /// </summary>
    public Claim ToClaim()
    {
        return new Claim(ClaimType, PermissionKey);
    }

    /// <summary>
    ///     Creates a permission from a claim
    /// </summary>
    public static Permission? FromClaim(Claim claim)
    {
        if (claim.Type != ClaimType)
            return null;

        return new Permission(claim.Value);
    }
}
