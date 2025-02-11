﻿using DotNetBrightener.Infrastructure.AppClientManager.Middlewares;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.Infrastructure.AppClientManager.JwtAuthentication;

public class AuthClientsAuthAudienceResolver : ICurrentRequestAudienceResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthClientsAuthAudienceResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string[] GetAudiences()
    {
        var appClientIdentityResult = _httpContextAccessor.RetrieveValue<AppClientIdentifyingResult>();

        if (appClientIdentityResult is null)
            return [];

        List<string> audiencesList = [];

        if (!string.IsNullOrEmpty(appClientIdentityResult.RequestFromAppBundleId))
        {
            audiencesList.AddRange(appClientIdentityResult.RequestFromAppBundleId.Split([";", ","],
                                                                                        StringSplitOptions
                                                                                           .RemoveEmptyEntries));
        }

        if (!string.IsNullOrEmpty(appClientIdentityResult.RequestFromAppDomain))
        {
            audiencesList.AddRange(appClientIdentityResult.RequestFromAppDomain.Split([";", ","],
                                                                                      StringSplitOptions
                                                                                         .RemoveEmptyEntries));
        }

        return audiencesList.Distinct()
                            .ToArray();
    }
}