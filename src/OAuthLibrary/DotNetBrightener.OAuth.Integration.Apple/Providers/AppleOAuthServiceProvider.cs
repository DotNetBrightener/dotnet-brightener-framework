using DotNetBrightener.OAuth.Integration.Apple.Models;
using DotNetBrightener.OAuth.Integration.Apple.Settings;
using DotNetBrightener.OAuth.Models;
using DotNetBrightener.OAuth.Providers;
using DotNetBrightener.OAuth.Settings;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.OAuth.Integration.Apple.Providers;

/// <summary>
/// 
/// </summary>
public class AppleOAuthServiceProvider : IOAuthServiceProvider
{
    private readonly IOAuthProviderSettingLoader<AppleOAuthSettings> _providerSettingLoader;

    public AppleOAuthServiceProvider(IOAuthProviderSettingLoader<AppleOAuthSettings> providerSettingLoader)
    {
        _providerSettingLoader = providerSettingLoader;
    }

    private const string OpenIdConfigRequest = "https://appleid.apple.com/.well-known/openid-configuration";
    
    /// <summary>
    /// 
    /// </summary>
    public string ProviderName => "apple";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oAuthRequestModel"></param>
    /// <returns></returns>
    public async Task<Uri> GetAuthenticationUrl(OAuthRequestModel oAuthRequestModel)
    {
        var openIdConfig = await ObtainAppleOpenIdConfig();
        
        var settings = await _providerSettingLoader.LoadSettings();

        var requestId = oAuthRequestModel.RequestId;

        var authenticationUrl = new UriBuilder(openIdConfig.authorization_endpoint);

        authenticationUrl.AddQueryParameters(new Dictionary<string, string>
        {
            {
                "response_type", "code id_token"
            },
            {
                "response_mode", "form_post"
            },
            {
                "client_id", settings.ClientId
            },
            {
                "redirect_uri", oAuthRequestModel.CallbackUrl
            },
            {
                "scope", string.Join(" ", openIdConfig.scopes_supported)
            },
            {
                "state", requestId
            }
        });

        return authenticationUrl.Uri;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentRequestUri"></param>
    /// <param name="originalRedirectUrl"></param>
    /// <param name="formCollection"></param>
    /// <returns></returns>
    public async Task<OAuthLogInResponse> AuthorizeFromCallback(Uri             currentRequestUri,
                                                    string          originalRedirectUrl,
                                                    IFormCollection formCollection)
    {
        formCollection.TryGetValue("code", out var code);
        formCollection.TryGetValue("user", out var userInfoString);

        var response = new OAuthLogInResponse();

        if (string.IsNullOrEmpty(code))
        {
            response.Status     = "Invalid Code";
            response.StatusCode = HttpStatusCode.Unauthorized;
            return response;
        }

        var settings = await _providerSettingLoader.LoadSettings();

        var clientSecret = await GenerateAppleClientSecret();
        var request = await GenerateRequestMessage(TokenType.AuthorizationCode,
                                                   code,
                                                   clientSecret,
                                                   settings.ClientId,
                                                   originalRedirectUrl);

        AuthorizationToken authToken = null;

        using (var httpclient = new HttpClient())
        {
            var responseMessage = await httpclient.SendAsync(request);
            var result          = await responseMessage.Content.ReadAsStringAsync();
            authToken = JsonConvert.DeserializeObject<AuthorizationToken>(result);
        }

        if (authToken is null ||
            string.IsNullOrEmpty(authToken.Token))
        {
            response.Status     = "Error while validating Apple account";
            response.StatusCode = HttpStatusCode.Unauthorized;

            return response;
        }

        var validatedTokenClaims = (await VerifyAppleIdToken(authToken.Token, settings.ClientId))?
           .ToArray();

        if (validatedTokenClaims?.Any() != true)
        {
            response.Status     = "Error while validating Apple account";
            response.StatusCode = HttpStatusCode.Unauthorized;

            return response;
        }

        // if verified, obtain email and proceed with email
        response.StatusCode = HttpStatusCode.OK;

        // Apple may omit name information due to its policy of protecting user personal data
        response.UserInformation = new OAuthUser
        {
            Email       = validatedTokenClaims.FirstOrDefault(_ => _.Type == AppleClaimConstants.Email)?.Value,
            ExternalKey = validatedTokenClaims.FirstOrDefault(_ => _.Type == AppleClaimConstants.Sub)?.Value,
        };

        if (!string.IsNullOrEmpty(userInfoString))
        {
            var userNameInfo = JsonConvert.DeserializeObject<AppleUserInfoModel>(userInfoString);

            if (userNameInfo is not null && 
                !string.IsNullOrEmpty(userNameInfo.Name.FirstName)) 
                // if user authenticates for the first time, Apple will provide the information
            {
                response.UserInformation.FirstName  = userNameInfo.Name.FirstName;
                response.UserInformation.MiddleName = userNameInfo.Name.MiddleName;
                response.UserInformation.LastName = userNameInfo.Name.LastName;
            }
        }

        return response;
    }

    public async Task<OAuthLogInResponse> AuthorizeFromClient(IFormCollection formCollection)
    {
        formCollection.TryGetValue("code", out var code);
        formCollection.TryGetValue("redirect_url", out var redirectUrl);
        formCollection.TryGetValue("full_name", out var fullNameObjString);

        var response = new OAuthLogInResponse();

        if (string.IsNullOrEmpty(code))
        {
            response.Status = "Invalid Code";
            response.StatusCode = HttpStatusCode.Unauthorized;
            return response;
        }

        var settings = await _providerSettingLoader.LoadSettings();

        var clientSecret = await GenerateAppleClientSecret(settings.MobileClientId);
        var request = await GenerateRequestMessage(TokenType.AuthorizationCode,
                                                   code,
                                                   clientSecret,
                                                   settings.MobileClientId,
                                                   redirectUrl);

        AuthorizationToken authToken = null;

        using (var httpclient = new HttpClient())
        {
            var responseMessage = await httpclient.SendAsync(request);
            var result = await responseMessage.Content.ReadAsStringAsync();
            authToken = JsonConvert.DeserializeObject<AuthorizationToken>(result);
        }

        if (authToken is null ||
            string.IsNullOrEmpty(authToken.Token))
        {
            response.Status = "Error while validating Apple account";
            response.StatusCode = HttpStatusCode.Unauthorized;

            return response;
        }

        var validatedTokenClaims = (await VerifyAppleIdToken(authToken.Token, settings.MobileClientId))?
           .ToArray();

        if (validatedTokenClaims?.Any() != true)
        {
            response.Status = "Error while validating Apple account";
            response.StatusCode = HttpStatusCode.Unauthorized;

            return response;
        }

        // if verified, obtain email and proceed with email
        response.StatusCode = HttpStatusCode.OK;

        // Apple may omit name information due to its policy of protecting user personal data
        response.UserInformation = new OAuthUser
        {
            Email = validatedTokenClaims.FirstOrDefault(_ => _.Type == AppleClaimConstants.Email)?.Value,
            ExternalKey = validatedTokenClaims.FirstOrDefault(_ => _.Type == AppleClaimConstants.Sub)?.Value,
        };

        if (!string.IsNullOrEmpty(fullNameObjString))
        {
            var userNameInfo = JsonConvert.DeserializeObject<AppleNameInfoClientObject>(fullNameObjString);

            if (userNameInfo is not null &&
                !string.IsNullOrEmpty(userNameInfo.GivenName))
                // if user authenticates for the first time, Apple will provide the information
            {
                response.UserInformation.FirstName  = userNameInfo.GivenName;
                response.UserInformation.MiddleName = userNameInfo.MiddleName;
                response.UserInformation.LastName   = userNameInfo.FamilyName;
            }
        }

        return response;
    }

    #region Private Methods
     
    public async Task<string> GenerateAppleClientSecret(string clientId = null)
    {
        var settings = await _providerSettingLoader.LoadSettings();
        var ecDsaCng = ECDsa.Create();

        ecDsaCng!.ImportPkcs8PrivateKey(Convert.FromBase64String(settings.PrivateKey), out var _);

        var signingCredentials = new SigningCredentials(new ECDsaSecurityKey(ecDsaCng), 
                                                        SecurityAlgorithms.EcdsaSha256);

        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(AppleClaimConstants.Issuer, settings.TeamId),
            new(AppleClaimConstants.IssuedAt, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64),
            new(AppleClaimConstants.Expiration, EpochTime.GetIntDate(now.AddMinutes(5)).ToString(), ClaimValueTypes.Integer64), 
            new(AppleClaimConstants.Audience, "https://appleid.apple.com"),
            new(AppleClaimConstants.Sub, clientId ?? settings.ClientId)
        };

        var token = new JwtSecurityToken(
                                         issuer: settings.TeamId,
                                         claims: claims,
                                         expires: now.AddMinutes(5),
                                         signingCredentials: signingCredentials);

        token.Header.Add(AppleClaimConstants.KeyID, settings.KeyId);

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private async Task<HttpRequestMessage> GenerateRequestMessage(string tokenType, 
                                                                  string authorizationCode,
                                                                  string clientSecret, 
                                                                  string clientId, 
                                                                  string redirectUrl = null)
    {
        var bodyAsPairs = new List<KeyValuePair<string, string>>()
        {
            new("client_id", clientId),
            new("client_secret", clientSecret),
            new("grant_type", tokenType),
        };

        if (!string.IsNullOrEmpty(redirectUrl))
            bodyAsPairs.Add(new("redirect_uri", redirectUrl));

        if (tokenType == TokenType.RefreshToken)
            bodyAsPairs.Add(new KeyValuePair<string, string>("refresh_token", authorizationCode));
        else
            bodyAsPairs.Add(new KeyValuePair<string, string>("code", authorizationCode));

        var content = new FormUrlEncodedContent(bodyAsPairs);

        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var openIdConfig = await ObtainAppleOpenIdConfig();

        return new HttpRequestMessage(HttpMethod.Post, openIdConfig.token_endpoint)
        {
            Content = content
        };
    }

    private async Task<IEnumerable<Claim>> VerifyAppleIdToken(string token, string clientId)
    {
        JwtSecurityTokenHandler tokenHandler      = new JwtSecurityTokenHandler();
        var                     deserializedToken = tokenHandler.ReadJwtToken(token);
        var                     claims            = deserializedToken.Claims
                                                                     .ToArray();

        SecurityKey publicKey;

        var expClaim = claims.FirstOrDefault(x => x.Type == AppleClaimConstants.Expiration)
                            ?.Value;

        if (string.IsNullOrEmpty(expClaim))
        {
            throw new SecurityTokenExpiredException("Expired token");
        }

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).DateTime;

        if (expirationTime < DateTime.UtcNow)
        {
            throw new SecurityTokenExpiredException("Expired token");
        }

        using (var httpClient = new HttpClient())
        {
            var applePublicKeys = await httpClient.GetStringAsync("https://appleid.apple.com/auth/keys");
            var keyset          = new JsonWebKeySet(applePublicKeys);

            publicKey = keyset.Keys.FirstOrDefault(x => x.Kid == deserializedToken.Header.Kid);

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer      = "https://appleid.apple.com",
                IssuerSigningKey = publicKey,
                ValidAudience    = clientId
            };

            tokenHandler.ValidateToken(token, validationParameters, out var _);

            return claims;
        }
    }

    private static async Task<AppleOpenIdConfig> ObtainAppleOpenIdConfig()
    {
        AppleOpenIdConfig openIdConfig = null;

        try
        {
            using (var httpClient = new HttpClient())
            {
                var requestUri = new UriBuilder(OpenIdConfigRequest);

                var response = await httpClient.GetStringAsync(requestUri.Uri);

                openIdConfig = JsonConvert.DeserializeObject<AppleOpenIdConfig>(response);
            }
        }
        catch (Exception)
        {
            openIdConfig = null;
        }

        if (openIdConfig is null)
            throw new InvalidOperationException($"Error while trying to obtain OpenID Configuration from Apple");

        return openIdConfig;
    }
    #endregion
}