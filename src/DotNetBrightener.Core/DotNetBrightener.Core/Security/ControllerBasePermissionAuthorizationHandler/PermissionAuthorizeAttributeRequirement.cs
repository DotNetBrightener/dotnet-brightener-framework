using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Core.Security.ControllerBasePermissionAuthorizationHandler
{
	/// <summary>
	/// Represent authorization requirement for authorize request using permission system
	/// </summary>
	public class PermissionAuthorizeAttributeRequirement : IAuthorizationRequirement
	{
		public const string PolicyName = "PermisssionPolicy";
	}
}