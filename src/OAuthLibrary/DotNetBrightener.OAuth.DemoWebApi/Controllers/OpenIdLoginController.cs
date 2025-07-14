using DotNetBrightener.OAuth.Controllers;
using DotNetBrightener.OAuth.Models;
using DotNetBrightener.OAuth.Providers;
using DotNetBrightener.OAuth.Services;
using DotNetBrightener.OAuth.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.OAuth.DemoWebApi.Controllers;

[Route("auth/openid")]
[ApiController]
public class OpenIdLoginController(
    IEnumerable<IOAuthServiceProvider> oAuthServiceProviders,
    IOptions<OAuthSettings>            oauthSettings,
    ILogger<OAuthController>           logger,
    IOAuthRequestManager               oAuthRequestManager,
    IHttpContextAccessor               httpContextAccessor)
    : OAuthController(oAuthServiceProviders,
                      oauthSettings,
                      logger,
                      oAuthRequestManager,
                      httpContextAccessor)
{
    protected override async Task<IActionResult> ProcessAuthenticatedExternalUser(ExternalLoginData externalLogin)
    {
        return Ok(externalLogin);
    }

    [HttpGet("connect/youtube")]
    public async Task<IActionResult> AuthorizeYoutube()
    {
        return await OAuthLogIn("google",
                                scopes:
                                [
                                    "https://www.googleapis.com/auth/youtube.readonly",
                                    "https://www.googleapis.com/auth/yt-analytics.readonly",
                                    "https://www.googleapis.com/auth/youtube.channel-memberships.creator"
                                ]);
    }

    protected override async Task OnExternalUserAuthenticated(OAuthLogInResponse externalLogin)
    {

    }
}