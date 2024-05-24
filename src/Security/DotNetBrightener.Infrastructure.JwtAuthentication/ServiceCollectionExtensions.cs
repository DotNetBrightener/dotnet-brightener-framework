using System.Data;
using System.Text;
using DotNetBrightener.CryptoEngine;
using DotNetBrightener.Infrastructure.JwtAuthentication.Middlewares;
using DotNetBrightener.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Enables all the authentication handlers to handle the authentication of the request
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAllAuthenticators(this IApplicationBuilder app)
    {
        app.UseCookiePolicy();

        return app.UseMiddleware<AllSchemesAuthenticationMiddleware>();
    }

    /// <summary>
    ///     Registers JWT Authentication to the service collection
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration"></param>
    /// <param name="includeCookieSupport"></param>
    /// <returns></returns>
    /// <exception cref="DataException"></exception>
    public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection serviceCollection,
                                                                   IConfiguration          configuration,
                                                                   string                  defaultScheme = JwtBearerDefaults.AuthenticationScheme,
                                                                   bool                    includeCookieSupport = true)
    {

        serviceCollection.AddSingleton<IAuthAudiencesContainer, DefaultAuthAudiencesContainer>();
        
        var tokenConfiguration = new JwtConfiguration
        {
            Issuer             = configuration.GetValue<string>("JwtTokenIssuer"),
            ExpireAfterMinutes = configuration.GetValue<int>("JwtExpireInMinutes")
        };

        var jwtPrivateKey = configuration.GetValue<string>("JwtTokenPrivateKey");

        if (string.IsNullOrEmpty(jwtPrivateKey))
        {
            tokenConfiguration.SignatureVerificationKey = configuration.GetValue<string>("JwtTokenSigningKey");
        }
        else
        {
            var rsa = RsaCryptoEngine.ImportPemPrivateKey(jwtPrivateKey);
            tokenConfiguration.PrivateSigningKey        = rsa.ExportPrivateKeyToPem(true);
            tokenConfiguration.SignatureVerificationKey = rsa.ExportPublicKeyToPem(true);
        }

        if (string.IsNullOrEmpty(tokenConfiguration.PrivateSigningKey) &&
            string.IsNullOrEmpty(tokenConfiguration.SignatureVerificationKey))
        {
            var signingKeyPairs = RsaCryptoEngine.GenerateKeyPair(true);

            Console.WriteLine($"JWT Private Signing Key = {signingKeyPairs.Item2}");

            throw new
                DataException("No JWT Signing Key is configured. " +
                              "Please check Admin Console to obtain the generated JWT Private Signing Key " +
                              "and configure it in appsettings.json or Environment Variable named 'JwtTokenPrivateKey'");
        }

        serviceCollection.AddSingleton(serviceProvider =>
        {
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            tokenConfiguration.ServiceScopeFactory = serviceScopeFactory;

            return tokenConfiguration;
        });

        serviceCollection.AddSingleton<IJwtMessageHandler, NullJwtMessageHandler>();

        serviceCollection.RegisterAuthAudienceResolver<NullCurrentRequestAudienceResolver>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var contextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

        var builder = serviceCollection.AddAuthentication(defaultScheme: defaultScheme)
                                       .AddJwtBearer(defaultScheme,
                                                     cfg => ConfigureJwtOptions(cfg, contextAccessor!));

        if (includeCookieSupport)
            builder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        return builder;
    }

    /// <summary>
    ///     Registers the audience validator for JWT to the service collection
    /// </summary>
    /// <typeparam name="TValidator">The type of the audience validator</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <returns>
    ///     The same instance of <paramref name="serviceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection RegisterAuthAudienceValidator<TValidator>(
        this IServiceCollection serviceCollection)
        where TValidator : class, IAuthAudienceValidator
    {
        serviceCollection.AddScoped<IAuthAudienceValidator, TValidator>();

        return serviceCollection;
    }

    /// <summary>
    ///     Registers the audience validator for JWT to the service collection
    /// </summary>
    /// <typeparam name="TResolver">The type of the audience validator</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <returns>
    ///     The same instance of <paramref name="serviceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection RegisterAuthAudienceResolver<TResolver>(this IServiceCollection serviceCollection)
        where TResolver : class, ICurrentRequestAudienceResolver
    {
        serviceCollection.AddScoped<ICurrentRequestAudienceResolver, TResolver>();

        return serviceCollection;
    }

    /// <summary>
    ///     Registers the JWT message event handler to the service collection
    /// </summary>
    /// <typeparam name="TMessageHandler"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterJwtMessageEventHandler<TMessageHandler>(
        this IServiceCollection serviceCollection)
        where TMessageHandler : class, IJwtMessageHandler
    {
        serviceCollection.AddSingleton<IJwtMessageHandler, TMessageHandler>();

        return serviceCollection;
    }

    private static void ConfigureJwtOptions(JwtBearerOptions     cfg,
                                            IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor?.HttpContext == null)
            return;

        var tokenConfiguration = httpContextAccessor.HttpContext
                                                    .RequestServices
                                                    .GetService<JwtConfiguration>();

        if (tokenConfiguration == null)
            throw new Exception("Cannot find valid configuration for auth token validation");

        var audienceValidator = httpContextAccessor.HttpContext
                                                   .RequestServices
                                                   .GetService<IAuthAudiencesContainer>();
        var jwtEventHandlers = httpContextAccessor.HttpContext
                                                  .RequestServices
                                                  .GetServices<IJwtMessageHandler>();

        // retrieve the key for verifying the token signature.
        // Although we use private key for signing, we only need public key to verify
        var signingKey = tokenConfiguration.SignatureVerificationKey;

        SecurityKey key = tokenConfiguration.UseRSASigningVerification
                              ? new RsaSecurityKey(RsaCryptoEngine.ImportPemPublicKey(signingKey))
                              : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        key.CryptoProviderFactory = new CryptoProviderFactory
        {
            CacheSignatureProviders = false
        };

        // what algorithm to verify the signature of the token
        var validAlgorithms = tokenConfiguration.UseRSASigningVerification
                                  ? SecurityAlgorithms.RsaSha256
                                  : SecurityAlgorithms.HmacSha256;

        if (cfg.Events == null)
            cfg.Events = new JwtBearerEvents();

        cfg.Events.OnMessageReceived = context =>
        {
            foreach (var jwtEventHandler in jwtEventHandlers)
            {
                jwtEventHandler.OnMessageReceived(context);

                if (!string.IsNullOrEmpty(context.Token))
                    break;
            }

            return Task.CompletedTask;
        };

        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = new List<SecurityKey>
            {
                key
            },
            ValidIssuers = new List<string>
            {
                tokenConfiguration.Issuer
            }.Distinct(),
            ValidAudiences          = audienceValidator!.ValidAudiences,
            TryAllIssuerSigningKeys = true,
            ValidAlgorithms         = [validAlgorithms],
            RoleClaimType           = CommonUserClaimKeys.UserRole,
            NameClaimType           = CommonUserClaimKeys.UserName
        };
    }
}