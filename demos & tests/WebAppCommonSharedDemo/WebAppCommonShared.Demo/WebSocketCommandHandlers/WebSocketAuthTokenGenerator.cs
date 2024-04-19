using System.Security.Claims;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using DotNetBrightener.WebSocketExt.Services;

namespace WebAppCommonShared.Demo.WebSocketCommandHandlers;

public class WebSocketAuthTokenGenerator : IWebSocketAuthTokenGenerator
{
    private readonly JwtConfiguration _jwtConfiguration;

    public WebSocketAuthTokenGenerator(JwtConfiguration jwtConfiguration)
    {
        _jwtConfiguration = jwtConfiguration;
    }

    public Task<WebSocketAuthenticationResult> GenerateToken(ClaimsPrincipal userPrincipal, TimeSpan expiresIn)
    {
        var token = _jwtConfiguration.CreateAuthenticationToken(userPrincipal.Claims.ToList(),
                                                                out var expiresAt,
                                                                expiresInMinutes: expiresIn.TotalMinutes);

        return Task.FromResult(new WebSocketAuthenticationResult
        {
            AuthenticationToken = token,
            ExpiresAt           = DateTimeOffset.FromUnixTimeMilliseconds((long)expiresAt)
        });
    }
}