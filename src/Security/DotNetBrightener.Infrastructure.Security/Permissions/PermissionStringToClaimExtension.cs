using DotNetBrightener.Infrastructure.Security.Permissions;

// ReSharper disable CheckNamespace

namespace System.Security.Claims;

public static class PermissionStringToClaimExtension
{
    public static Claim AsPermissionClaim(this string permissionKey)
    {
        return new Claim(Permission.ClaimType, permissionKey);
    }
}