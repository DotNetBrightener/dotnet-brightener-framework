using DotNetBrightener.OAuth.Integration.Google.Models;
using DotNetBrightener.OAuth.Integration.Google.Settings;
using DotNetBrightener.OAuth.Models;
using DotNetBrightener.OAuth.Providers;
using DotNetBrightener.OAuth.Settings;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.OAuth.Integration.Google.Providers;

public class GoogleOAuthServiceProvider(IOAuthProviderSettingLoader<GoogleOAuthSettings> providerSettingLoader)
    : IOAuthServiceProvider
{
    private const string ScopeProfile = "https://www.googleapis.com/auth/userinfo.profile";

    private const string ScopeEmail = "https://www.googleapis.com/auth/userinfo.email";

    private const string TokenRequestUrl = "https://accounts.google.com/o/oauth2/token";

    private const string EmailRequestUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

    public string ProviderName => "google";

    public async Task<Uri> GetAuthenticationUrl(OAuthRequestModel oAuthRequestModel)
    {
        var settings = await providerSettingLoader.LoadSettings();

        var requestId = oAuthRequestModel.RequestId;

        var authenticationUrl = new UriBuilder("https://accounts.google.com/o/oauth2/auth");

        var scopes = string.Join(" ",
                                 oAuthRequestModel.Scopes.Any()
                                     ? oAuthRequestModel.Scopes
                                     : [ScopeProfile, ScopeEmail]);

        authenticationUrl.AddQueryParameters(new Dictionary<string, string>
        {
            {
                "response_type", "code"
            },
            {
                "client_id", settings.ClientId
            },
            {
                "redirect_uri", oAuthRequestModel.CallbackUrl
            },
            {
                "scope", scopes
            },
            {
                "state", requestId
            }
        });

        return authenticationUrl.Uri;
    }

    public async Task<OAuthLogInResponse> AuthorizeFromCallback(Uri             currentRequestUri,
                                                                string          originalRedirectUrl,
                                                                IFormCollection formCollection)
    {
        var settings = await providerSettingLoader.LoadSettings();

        QueryHelpers.ParseQuery(currentRequestUri.Query)
                    .TryGetValue("code", out var code);

        var response = new OAuthLogInResponse();

        if (string.IsNullOrEmpty(code))
        {
            response.Status     = "Invalid Code";
            response.StatusCode = HttpStatusCode.Unauthorized;

            return response;
        }

        var token = await GetAccessToken(code,
                                         settings.ClientId,
                                         settings.ClientSecret,
                                         originalRedirectUrl);

        if (string.IsNullOrEmpty(token))
        {
            response.Status     = "Invalid Token";
            response.StatusCode = HttpStatusCode.Unauthorized;

            return response;
        }

        var accountInfo = await GetAccountInfo(token);

        if (accountInfo != null)
        {
            response.StatusCode = HttpStatusCode.OK;
            response.UserInformation = new OAuthUser
            {
                Email                  = accountInfo.Email,
                FirstName              = accountInfo.GivenName,
                LastName               = accountInfo.FamilyName,
                ExternalKey            = accountInfo.Id,
                ProfileImageUrl        = $"{accountInfo.Photo}?sz=360",
                ProfileImageUrlCropped = $"{accountInfo.Photo}?sz=100"
            };
        }
        else
        {
            response.Status     = "Error while getting account info";
            response.StatusCode = HttpStatusCode.InternalServerError;
        }

        return response;
    }

    public async Task<OAuthLogInResponse> AuthorizeFromClient(IFormCollection formCollection)
    {
        var response = new OAuthLogInResponse();

        if (formCollection.TryGetValue("user", out var userInfoString))
        {
            var accountInfo = JsonConvert.DeserializeObject<GoogleUserInformationClientModel>(userInfoString);

            if (accountInfo is not null)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.UserInformation = new OAuthUser
                {
                    Email                  = accountInfo.Email,
                    FirstName              = accountInfo.GivenName,
                    LastName               = accountInfo.FamilyName,
                    ExternalKey            = accountInfo.Id,
                    ProfileImageUrl        = $"{accountInfo.Photo.Replace("=s120", "=s512")}",
                    ProfileImageUrlCropped = $"{accountInfo.Photo}"
                };
            }
        }
        else
        {
            response.Status     = "Error while getting account info";
            response.StatusCode = HttpStatusCode.InternalServerError;
        }

        return response;
    }

    #region Private Methods

    private async Task<string> GetAccessToken(string code,
                                              string clientId,
                                              string clientSecret,
                                              string originalRedirectUrl)
    {
        try
        {
            var redirectUrl = originalRedirectUrl;

            var requestUrl = new UriBuilder(TokenRequestUrl);

            using (var httpClient = new HttpClient())
            {
                var formPostContent = new Dictionary<string, string>
                {
                    {
                        "code", code
                    },
                    {
                        "client_id", clientId
                    },
                    {
                        "client_secret", clientSecret
                    },
                    {
                        "redirect_uri", redirectUrl
                    },
                    {
                        "grant_type", "authorization_code"
                    }
                };

                var formUrlEncodedContent = new FormUrlEncodedContent(formPostContent);
                var response              = await httpClient.PostAsync(requestUrl.Uri, formUrlEncodedContent);

                response.EnsureSuccessStatusCode();
                var resultString = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<IDictionary<string, object>>(resultString);

                return result["access_token"].ToString();
            }
        }
        catch (Exception ex)
        {
            var    wex   = ex as WebException;
            string error = null;

            if (wex != null &&
                wex.Response != null)
            {
                using (var stream = wex.Response.GetResponseStream())
                {
                    if (stream != null)
                        using (var sr = new StreamReader(stream))
                            error = sr.ReadToEnd();
                }

                return error;
            }
        }

        return null;
    }

    private async Task<GoogleUserInformation> GetAccountInfo(string token)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var requestUri = new UriBuilder(EmailRequestUrl);
                requestUri.AddQueryString("access_token", token);

                var response = await httpClient.GetStringAsync(requestUri.Uri);

                var result = JsonConvert.DeserializeObject<GoogleUserInformation>(response);

                return result;
            }
        }
        catch (Exception exception)
        {

        }

        return null;
    }

    #endregion
}
