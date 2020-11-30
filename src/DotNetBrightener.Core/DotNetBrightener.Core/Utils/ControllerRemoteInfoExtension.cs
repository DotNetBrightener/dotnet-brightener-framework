using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.Core.Utils
{
	public static class ControllerRemoteInfoExtension
	{
		public static string GetRemoteIp(this Controller controller)
		{
			var remoteIpAddress = controller.Request.HttpContext.Connection.RemoteIpAddress;
			return remoteIpAddress.ToString();
		}
	}
}