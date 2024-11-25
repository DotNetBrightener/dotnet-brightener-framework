#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.JwtAuthentication.Middlewares;

public class AllSchemesAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AllSchemesAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes)
    {
        _next   = next ?? throw new ArgumentNullException(nameof(next));
        Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
    }

    /// <summary>
    /// Gets or sets the <see cref="IAuthenticationSchemeProvider"/>.
    /// </summary>
    public IAuthenticationSchemeProvider Schemes { get; set; }

    /// <summary>
    ///     Invokes the middleware performing authentication.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    public async Task Invoke(HttpContext                                 context,
                             ILogger<AllSchemesAuthenticationMiddleware> logger)
    {
        context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
        {
            OriginalPath     = context.Request.Path,
            OriginalPathBase = context.Request.PathBase
        });

        // Give any IAuthenticationRequestHandler schemes a chance to handle the request
        var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

        foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
        {
            if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler &&
                await handler.HandleRequestAsync())
            {
                logger.LogDebug("Request handled by {Scheme} handler.", scheme.Name);

                return;
            }
        }

        var authenticationSchemes = await Schemes.GetAllSchemesAsync();

        foreach (var scheme in authenticationSchemes)
        {
            logger.LogInformation("Processing authentication with {Scheme}", scheme.Name);
            var authenticateResult = await context.AuthenticateAsync(scheme.Name);

            if (authenticateResult.Principal != null)
            {
                context.User = authenticateResult.Principal;
                logger.LogInformation("AuthScheme {Scheme} successfully authenticated user", scheme.Name);

                var authFeatures = new AuthenticationFeatures(authenticateResult);

                context.Features.Set<IHttpAuthenticationFeature>(authFeatures);
                context.Features.Set<IAuthenticateResultFeature>(authFeatures);

                break;
            }
        }

        await _next(context);
    }
}

internal class AuthenticationFeatures : IAuthenticateResultFeature, IHttpAuthenticationFeature
{
    private ClaimsPrincipal?    _user;
    private AuthenticateResult? _result;

    public AuthenticationFeatures(AuthenticateResult result)
    {
        _result = result;
        _user   = _result?.Principal;
    }

    AuthenticateResult? IAuthenticateResultFeature.AuthenticateResult
    {
        get => _result;
        set
        {
            _result = value;
            _user   = _result?.Principal;
        }
    }

    ClaimsPrincipal? IHttpAuthenticationFeature.User
    {
        get => _user;
        set
        {
            _user   = value;
            _result = null;
        }
    }
}