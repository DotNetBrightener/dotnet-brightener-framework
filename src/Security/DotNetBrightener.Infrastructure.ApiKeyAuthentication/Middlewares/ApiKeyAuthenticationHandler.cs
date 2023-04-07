﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Constants;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;
using DotNetBrightener.Infrastructure.Security;
using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Middlewares;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyStoreService _apiKeyStoreService;
    private readonly IEventPublisher     _eventPublisher;

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                       ILoggerFactory                               logger,
                                       UrlEncoder                                   encoder,
                                       ISystemClock                                 clock,
                                       IApiKeyStoreService                          apiKeyStoreService,
                                       IEventPublisher                              eventPublisher)
        : base(options, logger, encoder, clock)
    {
        _apiKeyStoreService = apiKeyStoreService;
        _eventPublisher     = eventPublisher;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKeyValue) ||
            apiKeyValue.Count != 1)
        {
            Logger.LogWarning("An API request was received without the x-api-key header");

            return AuthenticateResult.Fail("No API Key Provided");
        }

        var apiKey = await _apiKeyStoreService.AuthorizeKey(apiKeyValue);

        if (apiKey == null)
        {
            Logger.LogWarning("An API request was received with an invalid x-api-key header");

            return AuthenticateResult.Fail($"Invalid API Key Provided");
        }

        var claims = new List<Claim>();

        if (apiKey.ExpiresAtUtc.HasValue)
            claims.Add(new Claim("exp", $"{apiKey.ExpiresAtUtc.Value.GetUnixTimestampInSeconds()}"));

        foreach (var apiKeyScope in apiKey.Scopes)
        {
            claims.Add(new Claim(Permission.ClaimType, apiKeyScope));
        }

        await _eventPublisher.Publish(new ApiKeyClaimProcessingEvent
        {
            Claims = claims,
            ApiKey = apiKey
        });

        var identity = new ClaimsIdentity(claims,
                                          ApiKeyAuthenticationOptions.AuthenticationScheme,
                                          nameType: CommonUserClaimKeys.UserName,
                                          roleType: CommonUserClaimKeys.UserRole);

        var identities = new List<ClaimsIdentity>
        {
            identity
        };
        var principal = new ClaimsPrincipal(identities);
        var ticket    = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.AuthenticationScheme);

        Logger.BeginScope($"{identity.Name}");
        Logger.LogInformation("Client authenticated with x-api-key header");

        return AuthenticateResult.Success(ticket);
    }
}