using System.Security.Claims;

namespace DotNetBrightener.Core.Permissions
{
    public static class PermissionStringToClaimExtension
    {
        public static Claim AsPermissionClaim(this string permissionKey)
        {
            return new Claim(Permission.ClaimType, permissionKey);
        }
    }
}