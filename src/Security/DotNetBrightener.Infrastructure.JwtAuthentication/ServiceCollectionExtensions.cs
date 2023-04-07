using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DotNetBrightener.CryptoEngine;
using DotNetBrightener.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public static class ServiceCollectionExtensions
{
    public static void AddJwtBearerAuthentication(this IServiceCollection serviceCollection,
                                                  IConfiguration          configuration)
    {

        serviceCollection
           .AddSingleton<IAuthAudiencesContainer, DefaultAuthAudiencesContainer>();

        serviceCollection.RegisterAuthAudienceValidator<DefaultAuthAudienceValidator>();

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
                DataException($"No JWT Signing Key is configured. " +
                              $"Please check Admin Console to obtain the generated JWT Private Signing Key " +
                              $"and configure it in appsettings.json or Environment Variable named 'JwtTokenPrivateKey'");
        }

        serviceCollection.AddSingleton(tokenConfiguration);
        
        var serviceProvider       = serviceCollection.BuildServiceProvider();
        var contextAccessor       = serviceProvider.GetService<IHttpContextAccessor>();

        serviceCollection.AddAuthentication()
                         .AddJwtBearer(cfg => ConfigureJwtOptions(cfg, contextAccessor!));
    }

    public static IServiceCollection RegisterAuthAudienceValidator<TValidator>(
        this IServiceCollection serviceCollection)
        where TValidator : class, IAuthAudienceValidator
    {
        serviceCollection.AddSingleton<IAuthAudienceValidator, TValidator>();

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
            ValidAlgorithms = new[]
            {
                validAlgorithms
            },
            RoleClaimType = CommonUserClaimKeys.UserRole,
            NameClaimType = CommonUserClaimKeys.UserName
        };
    }
}