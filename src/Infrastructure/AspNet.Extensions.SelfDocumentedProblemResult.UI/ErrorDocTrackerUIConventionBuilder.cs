using System;
using System.Collections.Generic;
using DotNetBrightener.Extensions.ProblemsResult.UI;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal class ErrorDocTrackerUIConventionBuilder : IEndpointConventionBuilder
{
    private readonly IEnumerable<IEndpointConventionBuilder> _endpoints;

    public ErrorDocTrackerUIConventionBuilder(IEnumerable<IEndpointConventionBuilder> endpoints)
    {
        _endpoints = Guard.ThrowIfNull(endpoints);
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var endpoint in _endpoints)
        {
            endpoint.Add(convention);
        }
    }
}