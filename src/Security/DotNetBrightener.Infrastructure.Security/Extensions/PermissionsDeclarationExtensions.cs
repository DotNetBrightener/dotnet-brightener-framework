using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Infrastructure.Security.Services;
using System.Reflection;

namespace DotNetBrightener.Infrastructure.Security.Extensions;

public static class PermissionsDeclarationExtensions
{
    /// <summary>
    ///     Extracts the permission information from given <paramref name="typeContainsPermissions" />
    /// </summary>
    /// <param name="typeContainsPermissions">
    ///     The <see cref="Type"/> contains the constant values of the permissions
    /// </param>
    /// <returns>
    ///     Array of permissions extracted from <paramref name="typeContainsPermissions"/>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the <paramref name="typeContainsPermissions"/> does not implement <see cref="IPermissionsDeclaration"/>
    /// </exception>
    public static Permission[] ExtractConstantsPermissions(this Type typeContainsPermissions)
    {
        if (!typeContainsPermissions.IsAssignableTo(typeof(IPermissionsDeclaration)))
            throw new
                ArgumentException("To extract permissions, the typeContainsPermissions must implement IPermissionsDeclaration interface",
                                  nameof(typeContainsPermissions));

        var typeInfo = typeContainsPermissions.GetTypeInfo();

        var declaredConstants = typeInfo.DeclaredFields
                                        .Where(f => f.FieldType.IsAssignableTo(typeof(string)));

        return declaredConstants.Select(constant => new Permission
                                 {
                                     PermissionKey = constant.GetRawConstantValue()?.ToString() ??
                                                     constant.GetValue(null)?.ToString(),
                                     Description = constant.GetXmlDocumentation()
                                 })
                                .Where(p => !string.IsNullOrEmpty(p.PermissionKey))
                                .ToArray();
    }
}