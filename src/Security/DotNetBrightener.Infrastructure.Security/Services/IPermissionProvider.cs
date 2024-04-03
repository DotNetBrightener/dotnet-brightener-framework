using DotNetBrightener.Infrastructure.Security.Permissions;

namespace DotNetBrightener.Infrastructure.Security.Services;

/// <summary>
///     Represents the provider that provides the permissions
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// Gets the name of the permission group
    /// </summary>
    string PermissionGroupName { get; }

    /// <summary>
    /// Returns the permissions that current provider provides
    /// </summary>
    /// <returns></returns>
    IEnumerable<Permission> GetPermissions();
}

/// <summary>
///     Marks the class with constant values as declaration of permissions
/// </summary>
public interface IPermissionsDeclaration {}