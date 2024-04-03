using DotNetBrightener.Infrastructure.Security.Permissions;

namespace DotNetBrightener.Infrastructure.Security.Services;

public interface IPermissionsContainer
{
    /// <summary>
    ///     Load all permissions from modules and validate for any duplicated permission key
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if permission key gets duplicated</exception>
    void LoadAndValidatePermissions();

    /// <summary>
    ///     Get all available permissions in the system
    /// </summary>
    /// <returns></returns>
    IEnumerable<Permission> GetAvailablePermissions();

    /// <summary>
    ///     Gets permission object by its key
    /// </summary>
    /// <param name="permissionKey">Key of the <see cref="Permission"/></param>
    /// <returns></returns>
    Permission GetPermissionByKey(string permissionKey);
}