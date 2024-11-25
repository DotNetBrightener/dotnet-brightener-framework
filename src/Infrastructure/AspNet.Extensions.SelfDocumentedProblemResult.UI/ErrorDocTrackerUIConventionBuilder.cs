

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal class ErrorDocTrackerUIConventionBuilder(IEnumerable<IEndpointConventionBuilder> endpoints)
    : IEndpointConventionBuilder
{
    private readonly IEnumerable<IEndpointConventionBuilder> _endpoints = Guard.ThrowIfNull(endpoints);

    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var endpoint in _endpoints)
        {
            endpoint.Add(convention);
        }
    }
}