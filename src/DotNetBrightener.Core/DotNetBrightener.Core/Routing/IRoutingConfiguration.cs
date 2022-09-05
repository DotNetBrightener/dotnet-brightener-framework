using System;
using Microsoft.AspNetCore.Routing;

namespace DotNetBrightener.Core.Routing;

/// <summary>
///		Represents the configurer to configure routings
/// </summary>
public interface IRoutingConfiguration
{
    int Order { get; }

    IRouter ConfigureRoute(IServiceProvider serviceProvider);
}