using System.Security.Claims;

namespace DotNetBrightener.Infrastructure.Security.Permissions;

public static class PermissionStringToClaimExtension
{
    public static Claim AsPermissionClaim(this string permissionKey)
    {
        return new Claim(Permission.ClaimType, permissionKey);
    }
}