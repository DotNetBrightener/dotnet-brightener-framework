using DotNetBrightener.OAuth.Models;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.OAuth.Providers;

/// <summary>
///     Represents the service that handles OAuth Authentication request
/// </summary>
public interface IOAuthServiceProvider
{
    /// <summary>
    ///     Name of the provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    ///     Retrieve the authentication URL for redirecting the request to
    /// </summary>
    /// <param name="oAuthRequestModel"></param>
    /// <returns></returns>
    Task<Uri> GetAuthenticationUrl(OAuthRequestModel oAuthRequestModel);

    /// <summary>
    ///     Process the OAuth response by the callback from the provider, via HTTP redirect or POST request
    /// </summary>
    /// <param name="currentRequestUri">The URL of current request</param>
    /// <param name="originalRedirectUrl">The original redirect URL assigned to the OAuth request</param>
    /// <param name="formCollection">The data sent by the POST request, if any</param>
    /// <returns></returns>
    Task<OAuthLogInResponse> AuthorizeFromCallback(Uri             currentRequestUri,
                                                   string          originalRedirectUrl,
                                                   IFormCollection formCollection);

    /// <summary>
    ///     Process the OAuth response from client
    /// </summary>
    /// <param name="formCollection">Data submitted from the client</param>
    /// <returns></returns>
    Task<OAuthLogInResponse> AuthorizeFromClient(IFormCollection formCollection);
}