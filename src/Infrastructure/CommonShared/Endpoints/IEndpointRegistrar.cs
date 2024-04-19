using Microsoft.AspNetCore.Routing;

namespace WebApp.CommonShared.Endpoints;

/// <summary>
///     Represents the endpoint registrar which is responsible for mapping the endpoints
/// </summary>
public interface IEndpointRegistrar
{
    /// <summary>
    ///    Maps the endpoints to the given <see cref="IEndpointRouteBuilder"/>
    /// </summary>
    /// <param name="app">
    ///     The <see cref="IEndpointRouteBuilder"/> to map the endpoints
    /// </param>
    void Map(IEndpointRouteBuilder app);
}