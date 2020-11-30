using System;
using System.Security.Claims;

namespace DotNetBrightener.Core.Permissions
{
	public class Permission
	{
		public const string ClaimType = "Permission";

        public Permission()
        {

        }

		public Permission(string permissionKey)
		{
			PermissionKey = permissionKey ?? throw new ArgumentNullException(nameof(permissionKey));
		}

		public Permission(string permissionKey, string description) : this(permissionKey)
		{
			Description = description;
		}

		public Permission(string permissionKey, string description, string permissionGroup) : this(permissionKey, description)
		{
			PermissionGroup = permissionGroup;
		}

		/// <summary>
		/// Specifies key of permission
		/// </summary>
		public string PermissionKey { get; set; }

		/// <summary>
		/// Specifies the group of permission
		/// </summary>
		public string PermissionGroup { get; set; }

		/// <summary>
		/// Describe permission information
		/// </summary>
		public string Description { get; set; }

		public bool EnabledByDefault { get; set; }


		public static implicit operator Claim(Permission p)
		{
			return new Claim(ClaimType, p.PermissionKey);
		}

		public static implicit operator Permission(string permissionKey)
		{
			return new Permission(permissionKey);
		}
	}

    public static class PermissionStringToClaimExtension
    {
        public static Claim AsPermissionClaim(this string permissionKey)
        {
            return (Permission) permissionKey;
        }
    }
}