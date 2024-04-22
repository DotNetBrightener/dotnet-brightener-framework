using DotNetBrightener.Infrastructure.JwtAuthentication;
using DotNetBrightener.WebSocketExt.Authentication;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBearerWebSocketExtensions
{
    public static IServiceCollection AddWebSocketJwtBearerMessageHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterJwtMessageEventHandler<WebSocketJwtBearerMessageHandler>();

        return serviceCollection;
    }
}