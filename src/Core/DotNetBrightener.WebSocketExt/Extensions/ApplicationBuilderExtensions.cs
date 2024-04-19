using DotNetBrightener.WebSocketExt;
using DotNetBrightener.WebSocketExt.Middlewares;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    public static IEndpointRouteBuilder UseWebSocketAuthRequestEndpoint(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<WebSocketExtOptions>>();


        app.MapPost(options.Value.AuthInitializePath,
                    async (HttpContext                 context,
                           IWebSocketUserAuthExchanger webSocketUserAuthExchanger) =>
                    {
                        var connectionToken = await webSocketUserAuthExchanger.ExchangeAuthTokenForLongAccess(context.User);

                        return Results.Ok(connectionToken);
                    })
           .RequireAuthorization();

        return app;
    }

    /// <summary>
    ///     Enables the websocket endpoint for the application
    /// </summary>
    /// <param name="app"></param>
    public static IApplicationBuilder UseWebSocketCommandServices(this IApplicationBuilder app)
    {

        var options = app.ApplicationServices.GetRequiredService<IOptions<WebSocketExtOptions>>();

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(options.Value.KeepAliveIntervalInSeconds)
        };

        app.UseWebSockets(webSocketOptions);

        app.UseMiddleware<WebSocketBasedServiceMiddleware>();

        return app;
    }
}