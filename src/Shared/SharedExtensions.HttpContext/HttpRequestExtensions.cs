/***
 *
 * Copyright (c) 2021 DotNetBrightener.
 * Licensed under MIT.
 * Feel free to use!!
 * https://gist.github.com/dotnetbrightener/e9bdacd19714685b24e143e5600fc2ee
 ***/

using System;
using Microsoft.AspNetCore.Http.Extensions;

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

        if (request.Headers.ContainsKey("x-forwarded-host"))
        {
            request.Headers.TryGetValue("x-forwarded-host", out var host);
            var uri = new UriBuilder(requestUrl)
            {
                Host = host,
            };
            uri.Port = uri.Scheme == "http" ? 80 : 443;
            requestUrl = new Uri(uri.ToString()).GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port,
                                                               UriFormat.UriEscaped);
        }

        return requestUrl;
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
        if (httpContextAccessor?.HttpContext?.Request == null)
            return string.Empty;

        return httpContextAccessor.HttpContext.Request.GetRequestUrl();
    }
}