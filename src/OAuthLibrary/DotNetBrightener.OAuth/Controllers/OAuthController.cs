using DotNetBrightener.OAuth.Models;
using DotNetBrightener.OAuth.Providers;
using DotNetBrightener.OAuth.Services;
using DotNetBrightener.OAuth.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.OAuth.Controllers;

[AllowAnonymous]
public abstract class OAuthController(
    IEnumerable<IOAuthServiceProvider> oAuthServiceProviders,
    IOptions<OAuthSettings>            oauthSettings,
    ILogger<OAuthController>           logger,
    IOAuthRequestManager               oAuthRequestManager,
    IHttpContextAccessor               httpContextAccessor)
    : Controller
{
    protected readonly OAuthSettings                      OauthSettings         = oauthSettings.Value;
    protected readonly IOAuthRequestManager               OAuthRequestManager   = oAuthRequestManager;
    protected readonly ILogger                            Logger                = logger;
    protected readonly IHttpContextAccessor               HttpContextAccessor   = httpContextAccessor;
    protected readonly IEnumerable<IOAuthServiceProvider> OAuthServiceProviders = oAuthServiceProviders;

    /// <summary>
    /// Initiates the login process
    /// </summary>
    /// <param name="externalLoginProvider">The OAuth provider name (e.g., 'google', 'apple')</param>
    /// <param name="redirectUrl">Optional redirect URL for mobile clients (query parameter)</param>
    /// <param name="isMobile">Optional flag to indicate mobile client (query parameter)</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpGet, Route("{externalLoginProvider}")]
    public async Task<IActionResult> OAuthLogIn(string externalLoginProvider,
                                                [FromQuery] string redirectUrl = null,
                                                [FromQuery] bool isMobile = false)
    {
        var oauthServiceProvider = RetrieveOAuthProviderOrThrow(externalLoginProvider);

        var requestUrl  = new Uri(Request.GetDisplayUrl());
        var callbackUri = new Uri(requestUrl, requestUrl.AbsolutePath + "/callback");

        if (OauthSettings.ForceHttps &&
            callbackUri.Scheme == Uri.UriSchemeHttp)
        {
            var callbackUrl = new UriBuilder(callbackUri.ToString())
            {
                Scheme = Uri.UriSchemeHttps,
                Port   = -1
            };

            callbackUri = callbackUrl.Uri;
        }

        var callback = $"{callbackUri}";

        Request.Query.TryGetValue("validateUserOnly", out var validateUserOnly);

        // Enhanced redirect URL handling for mobile and web clients
        var finalRedirectUrl = DetermineRedirectUrl(redirectUrl, isMobile);

        var oAuthRequestModel = new OAuthRequestModel
        {
            RequestId        = Guid.NewGuid().ToString(),
            CallbackUrl      = callback,
            LinkedUserId     = null,
            ValidateUserOnly = validateUserOnly.ToString()
                                               .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase),
            RedirectUrl      = finalRedirectUrl
        };

        // Add mobile flag to extra parameters for callback processing
        oAuthRequestModel.ExtraParameters.Add("isMobile", isMobile.ToString());

        foreach (var keyValuePair in Request.Query)
        {
            if (!keyValuePair.Key.Equals("redirectUrl", StringComparison.OrdinalIgnoreCase) &&
                !keyValuePair.Key.Equals("isMobile", StringComparison.OrdinalIgnoreCase))
            {
                oAuthRequestModel.ExtraParameters.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        var url = await oauthServiceProvider.GetAuthenticationUrl(oAuthRequestModel);

        OAuthRequestManager.CacheOAuthRequest(oAuthRequestModel);

        return Redirect(url.AbsoluteUri);
    }

    /// <summary>
    ///     The callback method of what to do once we receive a response form the oauth provider
    /// </summary>
    /// <param name="externalLoginProvider"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpGet, HttpPost, Route("{externalLoginProvider}/callback")]
    public async Task<IActionResult> OAuthLogInCallback(string externalLoginProvider)
    {
        var oauthServiceProvider = RetrieveOAuthProviderOrThrow(externalLoginProvider);

        var requestUrl        = new Uri(Request.GetDisplayUrl());
        var oAuthRequestModel = FindOAuthRequest(requestUrl);

        if (oAuthRequestModel == null)
        {
            throw new InvalidOperationException("Request is not valid");
        }

        var originalCallbackUrl = oAuthRequestModel.CallbackUrl;

        var formData = Request.Method == HttpMethods.Post
                           ? Request.Form
                           : null;

        var result = await oauthServiceProvider.AuthorizeFromCallback(requestUrl,
                                                                      originalCallbackUrl,
                                                                      formData);

        // Check if this is a mobile request
        var isMobile = oAuthRequestModel.ExtraParameters.ContainsKey("isMobile") &&
                       bool.TryParse(oAuthRequestModel.ExtraParameters["isMobile"].ToString(), out var mobileFlag) &&
                       mobileFlag;

        if (result.StatusCode != HttpStatusCode.OK)
        {
            return HandleAuthenticationError(externalLoginProvider, oAuthRequestModel, isMobile);
        }

        if (isMobile)
        {
            return HandleMobileCallback(result, oAuthRequestModel);
        }
        else
        {
            return HandleWebCallback(result, oAuthRequestModel);
        }
    }

    [HttpPost("{externalLoginProvider}/verifyOAuthResponse")]
    public async Task<IActionResult> VerifyOAuthResponse(string externalLoginProvider)
    {
        var oauthServiceProvider = RetrieveOAuthProviderOrThrow(externalLoginProvider);

        var formData = Request.Form;

        var result = await oauthServiceProvider.AuthorizeFromClient(formData);
        
        return await AuthenticateOAuthUser(externalLoginProvider, result);
    }

    [HttpPost("AuthenticateOAuthUser/{externalLoginProvider}")]
    public async Task<IActionResult> AuthenticateOAuthUser(string externalLoginProvider,
                                                           [FromBody]
                                                           OAuthUser model)
    {
        var oAuthLogin = new OAuthLogInResponse
        {
            Status          = string.Empty,
            StatusCode      = HttpStatusCode.OK,
            UserInformation = model
        };

        return await AuthenticateOAuthUser(externalLoginProvider, oAuthLogin);
    }

    protected abstract Task<IActionResult> ProcessAuthenticatedExternalUser(ExternalLoginData externalLogin);

    #region Private Methods

    private IOAuthServiceProvider RetrieveOAuthProviderOrThrow(string externalLoginProvider)
    {
        Func<IOAuthServiceProvider, bool> query;

        query = x => x.ProviderName.Equals(externalLoginProvider,
                                           StringComparison.OrdinalIgnoreCase);

        var oauthServiceProvider = OAuthServiceProviders.FirstOrDefault(query);

        if (oauthServiceProvider == null)
            throw new InvalidOperationException("The provided service for OAuth login is not available");

        return oauthServiceProvider;
    }

    private OAuthRequestModel FindOAuthRequest(Uri requestRequestUri)
    {
        StringValues stateQueryString = StringValues.Empty;

        if (Request.Method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            QueryHelpers.ParseQuery(requestRequestUri.Query)
                        .TryGetValue("state", out stateQueryString);
        } 
        else if (Request.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
        {
            Request.Form.TryGetValue("state", out stateQueryString);
        }

        if (StringValues.IsNullOrEmpty(stateQueryString))
            throw new InvalidOperationException($"Cannot verify request");

        var requestId = stateQueryString.ToString()
                                        .Split(new[]
                                               {
                                                   "||"
                                               },
                                               StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault();

        var oauthRequest = OAuthRequestManager.GetOAuthRequestAndRemove(requestId);

        if (oauthRequest == null)
        {
            throw new KeyNotFoundException($"Could not find the given state ({requestId}) in the session");
        }

        return oauthRequest;
    }

    private async Task<IActionResult> AuthenticateOAuthUser(string             externalLoginProvider,
                                                            OAuthLogInResponse result,
                                                            OAuthRequestModel  oAuthRequestModel = null)
    {
        var externalLogin = new ExternalLoginData
        {
            ExternalAccessToken = result.UserInformation.AccessToken,
            FirstName = result.UserInformation.FirstName,
            LastName = result.UserInformation.LastName,
            UserName = result.UserInformation.Email,
            ExternalId = result.UserInformation.ExternalKey,
            ProviderName = externalLoginProvider,
            ProfileImageUrl = result.UserInformation.ProfileImageUrl,
            ProfileImageUrlCropped = result.UserInformation.ProfileImageUrlCropped,
            LinkedUserId = oAuthRequestModel?.LinkedUserId,
            ExtraParameters = oAuthRequestModel?.ExtraParameters
        };

        return await ProcessAuthenticatedExternalUser(externalLogin);
    }

    /// <summary>
    /// Determines the appropriate redirect URL based on client type and provided parameters
    /// </summary>
    /// <param name="providedRedirectUrl">Redirect URL provided by the client</param>
    /// <param name="isMobile">Whether this is a mobile client</param>
    /// <returns>The final redirect URL to use</returns>
    private string DetermineRedirectUrl(string providedRedirectUrl, bool isMobile)
    {
        // If a redirect URL is explicitly provided, use it
        if (!string.IsNullOrEmpty(providedRedirectUrl))
        {
            return providedRedirectUrl;
        }

        // For web clients, try to use the Referer header
        if (!isMobile && !string.IsNullOrEmpty(Request.Headers.Referer))
        {
            return Request.Headers.Referer;
        }

        // For mobile clients or when Referer is not available, use a default mobile scheme
        if (isMobile)
        {
            return OauthSettings.DefaultMobileRedirectUrl ?? "myapp://oauth/callback";
        }

        // Fallback for web clients when no Referer is available
        return OauthSettings.DefaultWebRedirectUrl ?? Request.GetDisplayUrl().Split('?')[0];
    }

    /// <summary>
    /// Handles authentication errors for both web and mobile clients
    /// </summary>
    private IActionResult HandleAuthenticationError(string externalLoginProvider, OAuthRequestModel oAuthRequestModel, bool isMobile)
    {
        var errorMessage = $"Error while authenticating with {externalLoginProvider}";

        if (isMobile)
        {
            // For mobile, redirect to the mobile app with error parameters
            var mobileRedirectUrl = new UriBuilder(oAuthRequestModel.RedirectUrl);
            mobileRedirectUrl.AddQueryParameters(new Dictionary<string, string>
            {
                { "error", "authentication_failed" },
                { "error_description", errorMessage }
            });

            return Redirect(mobileRedirectUrl.Uri.AbsoluteUri);
        }
        else
        {
            // For web, use the existing web error handling
            var redirectUrl = new UriBuilder(oAuthRequestModel.RedirectUrl);
            redirectUrl.AddQueryParameters(new Dictionary<string, string>
            {
                { "error_message", errorMessage }
            });

            return Redirect(redirectUrl.Uri.AbsoluteUri);
        }
    }

    /// <summary>
    /// Handles successful authentication callback for mobile clients
    /// </summary>
    private IActionResult HandleMobileCallback(OAuthLogInResponse result, OAuthRequestModel oAuthRequestModel)
    {
        // For mobile clients, redirect to the mobile app with user data as query parameters
        var mobileRedirectUrl = new UriBuilder(oAuthRequestModel.RedirectUrl);

        var userData = new Dictionary<string, string>
        {
            { "success", "true" },
            { "email", result.UserInformation.Email ?? "" },
            { "firstName", result.UserInformation.FirstName ?? "" },
            { "lastName", result.UserInformation.LastName ?? "" },
            { "externalKey", result.UserInformation.ExternalKey ?? "" },
            { "profileImageUrl", result.UserInformation.ProfileImageUrl ?? "" }
        };

        mobileRedirectUrl.AddQueryParameters(userData);

        return Redirect(mobileRedirectUrl.Uri.AbsoluteUri);
    }

    /// <summary>
    /// Handles successful authentication callback for web clients
    /// </summary>
    private IActionResult HandleWebCallback(OAuthLogInResponse result, OAuthRequestModel oAuthRequestModel)
    {
        // For web clients, use the existing JavaScript postMessage approach
        var serializedData = JsonConvert.SerializeObject(new
                                                         {
                                                             eventType = "OAuthResult",
                                                             isSuccess = true,
                                                             userData  = result.UserInformation
                                                         },
                                                         new JsonSerializerSettings
                                                         {
                                                             ContractResolver =
                                                                 new CamelCasePropertyNamesContractResolver()
                                                         });

        var redirectUrl = new UriBuilder(oAuthRequestModel.RedirectUrl);
        var targetOrigin = new Uri(redirectUrl.Uri, "/");

        var replace = OAuthResponseScripts.OAuthResponseScript
                                          .Replace(OAuthResponseScripts.ResponseSerializeStringToken,
                                                   serializedData)
                                          .Replace(OAuthResponseScripts.TargetOriginToken,
                                                   targetOrigin.ToString());

        return Content(replace, "text/html");
    }

    #endregion
}
