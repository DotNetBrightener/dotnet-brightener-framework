/***
 *
 * Copyright (c) 2022 DotNetBrightener.
 * Licensed under MIT.
 * Feel free to use!!
 * https://gist.github.com/JustinChasez/37c7c9ea910c191ff5a15e9fd7ae53ba
 ***/

using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http;

internal static class HttpRequestExtensions
{
    /// <summary>
    ///     Retrieves the actual request URL, just in case the server side is behind a reverse proxy
    /// </summary>
    /// <param name="request">
    /// </param>
    /// <returns>
    ///     The actual URL sent from the browser request
    /// </returns>
    public static string GetRequestUrl(this HttpRequest request)
    {
        var requestUrl = request.GetDisplayUrl();

        var uri  = new UriBuilder(requestUrl);
        
        if (request.Headers.ContainsKey("X-Forwarded-Proto"))
        {
            request.Headers.TryGetValue("X-Forwarded-Proto", out var proto);
            uri.Scheme = proto;
            uri.Port   = uri.Scheme == "http" ? 80 : 443;
        }

        requestUrl = new Uri(uri.ToString()).GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port,
                                                           UriFormat.UriEscaped);

        return requestUrl;
    }

    /// <summary>
    ///     Retrieves the IP Address from the client for the current Http Request
    /// </summary>
    /// <returns></returns>
    public static string GetClientIP(this HttpContext currentContext)
    {
        if (currentContext is null)
            return null;

        StringValues ipString;

        if (!currentContext.Request.Headers.TryGetValue("X-Forwarded-For", out ipString) &&
            !currentContext.Request.Headers.TryGetValue("X-Real-IP", out ipString))
            return currentContext.Connection?.RemoteIpAddress?.ToString();

        if (ipString.Contains(","))
            ipString = ipString.ToString()
                               .Split(',')
                               .First()
                               .Trim();
        return ipString;
    }

    /// <summary>
    ///     Retrieves the actual request URL, just in case the server side is behind a reverse proxy
    /// </summary>
    /// <param name="httpContextAccessor">
    /// </param>
    /// <returns>
    ///     The actual URL sent from the browser request
    /// </returns>
    public static string GetRequestUrl(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor?.HttpContext?.Request == null 
                   ? string.Empty 
                   : httpContextAccessor.HttpContext.Request.GetRequestUrl();
    }
}