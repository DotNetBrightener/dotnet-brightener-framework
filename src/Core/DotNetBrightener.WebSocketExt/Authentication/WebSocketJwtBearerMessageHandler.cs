using DotNetBrightener.Infrastructure.JwtAuthentication;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.WebSocketExt.Authentication;

internal class WebSocketJwtBearerMessageHandler : IJwtMessageHandler
{
    private readonly IOptions<WebSocketExtOptions> _options;

    public WebSocketJwtBearerMessageHandler(IOptions<WebSocketExtOptions> options)
    {
        this._options = options;
    }

    public void OnMessageReceived(MessageReceivedContext context)
    {
        var httpContext = context.HttpContext;

        if (httpContext.Request.Path != _options.Value.Path)
        {
            return;
        }

        if (!httpContext.Request.Query.TryGetValue(_options.Value.ConnectionTokenQueryParamName, out var token) ||
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