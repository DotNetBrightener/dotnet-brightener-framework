using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Infrastructure.Security.Requirements;

/// <summary>
///     Represents a requirement of authorizing multiple specified permissions
/// </summary>
public class PermissionsAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes the permission requirement with list of permission keys
    /// </summary>
    /// <param name="permissionKeys">Keys of the permissions</param>
    public PermissionsAuthorizationRequirement(string permissionKeys)
        : this(permissionKeys.Split([
                                        ";", ","
                                    ],
                                    StringSplitOptions.RemoveEmptyEntries))
    {
    }

    /// <summary>
    ///     Initializes the permission requirement with list of permission keys
    /// </summary>
    /// <param name="permissionKeys">Keys of the permissions</param>
    public PermissionsAuthorizationRequirement(string [ ] permissionKeys)
    {
        PermissionsToAuthorize = permissionKeys;
    }

    public string [ ] PermissionsToAuthorize
    {
        get;
        private set;
    }
}