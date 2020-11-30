using System;
using Microsoft.AspNetCore.Routing;

namespace DotNetBrightener.Core.Routing
{
	public interface IRoutingConfiguration
	{
		int Order { get; }

		IRouter ConfigureRoute(IServiceProvider serviceProvider);
	}
}