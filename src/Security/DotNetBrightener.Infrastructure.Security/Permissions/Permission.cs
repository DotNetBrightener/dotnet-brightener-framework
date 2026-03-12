using System.Security.Claims;

namespace DotNetBrightener.Infrastructure.Security.Permissions;

/// <summary>
///		Represents a permission
/// </summary>
public class Permission
{
    /// <summary>
    ///		The identifier of for the <see cref="Claim" /> that represents the permission claim
    /// </summary>
    public const string ClaimType = "Permission";

    public Permission()
    {

    }

    public Permission(string                  permissionKey,
                      string                  description     = null,
                      string                  permissionGroup = null,
                      IEnumerable<Permission> inheritedFrom   = null)
    {
        if (string.IsNullOrEmpty(permissionKey))
        {
            throw new ArgumentNullException(nameof(permissionKey));
        }

        PermissionKey   = permissionKey;
        Description     = description;
        PermissionGroup = permissionGroup;
        InheritedFrom   = inheritedFrom ?? [];
    }

    /// <summary>
    ///		Specifies key of permission
    /// </summary>
    public string PermissionKey { get; set; }

    /// <summary>
    ///		Specifies the group of permission
    /// </summary>
    public string PermissionGroup { get; set; }

    /// <summary>
    ///		Gets or sets the collection of permissions that should exist in order for this permission to be authorized
    /// </summary>
    public IEnumerable<Permission> InheritedFrom { get; }

    /// <summary>
    ///		Describe permission information
    /// </summary>
    public string Description { get; set; }
}