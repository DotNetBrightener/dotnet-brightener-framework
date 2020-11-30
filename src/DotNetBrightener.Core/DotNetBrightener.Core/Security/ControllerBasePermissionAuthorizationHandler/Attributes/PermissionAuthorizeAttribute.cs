using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Core.Security.ControllerBasePermissionAuthorizationHandler.Attributes
{
	/// <summary>
	/// Marks the controller/action to be authorized but not rely on permissions
	/// </summary>
	public class NonPermissionAuthorizeAttribute : AuthorizeAttribute
	{

	}

	/// <summary>
	/// Mark the controller/action to be authorized using <see cref="PermissionAuthorizeAttributeHandler"/>
	/// </summary>
	public class PermissionAuthorizeAttribute : AuthorizeAttribute
	{
		public string Permissions { get; set; }

		public PermissionAuthorizeAttribute() : base(PermissionAuthorizeAttributeRequirement.PolicyName)
		{

		}

		public PermissionAuthorizeAttribute(string permission) : base(PermissionAuthorizeAttributeRequirement.PolicyName)
		{
			Permissions = permission;
		}

		public IEnumerable<string> GetPermissions()
		{
			return !string.IsNullOrEmpty(Permissions)
					   ? Permissions.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries)
					   : new string[] { };
		}
	}
}