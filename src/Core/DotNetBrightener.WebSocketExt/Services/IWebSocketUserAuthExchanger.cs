using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.WebSocketExt.Services;

/// <summary>
///     Represents the service that exchanges the user's auth token for a long access token.
/// </summary>
internal interface IWebSocketUserAuthExchanger
{
    Task<string> ExchangeAuthTokenForLongAccess(ClaimsPrincipal userPrincipal);
}

internal class DefaultWebSocketUserAuthExchanger : IWebSocketUserAuthExchanger
{
    private readonly IConnectionManager           _connectionManager;
    private readonly IWebSocketAuthTokenGenerator _authTokenGenerator;
    private readonly WebSocketExtOptions          _options;

    public DefaultWebSocketUserAuthExchanger(IConnectionManager            connectionManager,
                                             IWebSocketAuthTokenGenerator  authTokenGenerator,
                                             IOptions<WebSocketExtOptions> options)
    {
        _connectionManager  = connectionManager;
        _authTokenGenerator = authTokenGenerator;
        _options            = options.Value;
    }

    public async Task<string> ExchangeAuthTokenForLongAccess(ClaimsPrincipal userPrincipal)
    {
        var connectionId = Guid.NewGuid().ToString();

        var expiresIn = TimeSpan.FromMinutes(_options.TimeToLiveAccessTokenInMinutes);

        var authResult = await _authTokenGenerator.GenerateToken(userPrincipal, expiresIn);

        var connectionInfo = new ConnectionInfo
        {
            AuthToken     = authResult.AuthenticationToken,
            ConnectionId  = connectionId,
            UserPrincipal = userPrincipal,
            ExpiresAt     = authResult.ExpiresAt
        };

        await _connectionManager.AddConnection(connectionId, connectionInfo);

        return connectionId;
    }
}