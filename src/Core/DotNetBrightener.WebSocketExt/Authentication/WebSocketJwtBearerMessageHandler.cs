using DotNetBrightener.Infrastructure.JwtAuthentication;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.WebSocketExt.Authentication;

internal class WebSocketJwtBearerMessageHandler(IOptions<WebSocketExtOptions> options) : IJwtMessageHandler
{
    public void OnMessageReceived(MessageReceivedContext context)
    {
        var httpContext = context.HttpContext;

        if (httpContext.Request.Path != options.Value.Path)
        {
            return;
        }

        if (!httpContext.Request.Query.TryGetValue(options.Value.ConnectionTokenQueryParamName, out var token) ||
            string.IsNullOrEmpty(token))
        {
            return;
        }

        var connectionManager = context.HttpContext.RequestServices.GetRequiredService<IConnectionManager>();

        if (connectionManager.TryGetConnection(token.ToString(), out var connectionInfo))
        {
            if (connectionInfo is not null &&
                !string.IsNullOrEmpty(connectionInfo.AuthToken))
                context.Token = connectionInfo.AuthToken;
        }
    }
}