using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Models;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;
using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Constants;
using DotNetBrightener.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Middlewares;

public class ApiKeyClaimProcessingEvent : IEventMessage
{
    public List<Claim> Claims { get; set; }
    public ApiKey      ApiKey { get; set; }
}

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(HttpContext         context,
                             IApiKeyStoreService apiKeyStoreService,
                             IEventPublisher     eventPublisher)
    {
        var hasAuthorizationHeader = context.Request.Headers.ContainsKey(ApiKeyAuthConstants.ApiTokenHeaderKey);

        if (hasAuthorizationHeader &&
            context.Request.Headers.TryGetValue(ApiKeyAuthConstants.ApiTokenHeaderKey, out var apiKeyValue))
        {
            var apiKey = await apiKeyStoreService.AuthorizeKey(apiKeyValue);

            if (apiKey == null)
            {
                await _next(context);

                return;
            }

            var claims = new List<Claim>();

            if (apiKey.ExpiresAtUtc.HasValue)
                claims.Add(new Claim("exp", $"{apiKey.ExpiresAtUtc.Value.GetUnixTimestampInSeconds()}"));

            foreach (var apiKeyScope in apiKey.Scopes)
            {
                claims.Add(new Claim(Permission.ClaimType, apiKeyScope));
            }

            await eventPublisher.Publish(new ApiKeyClaimProcessingEvent
            {
                Claims = claims,
                ApiKey = apiKey
            });

            var claimIdentity = new ClaimsPrincipal(new ClaimsIdentity(claims,
                                                                       ApiKeyAuthConstants.ApiTokenAuthScheme,
                                                                       nameType: CommonUserClaimKeys.UserName,
                                                                       roleType: CommonUserClaimKeys.UserRole));

            context.User = claimIdentity;
        }

        await _next(context);
    }
}