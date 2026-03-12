#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetBrightener.CryptoEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public static class JwtConfigurationExtensions
{
    /// <summary>
    ///     Generates the JWT token for authenticating from given <see cref="claims"/>
    /// </summary>
    /// <param name="jwtConfiguration">The <see cref="JwtConfiguration"/> object</param>
    /// <param name="claims">The claims that form the Identity</param>
    /// <param name="expiresAt">Specifies when the generated token expires</param>
    /// <param name="audiencesString">Specifies the audiences for the generated token</param>
    /// <param name="expiresInMinutes">
    ///         Specifies the minutes after generated to expire the token
    /// </param>
    /// <param name="appendData">
    ///     Performs more actions to the token before generating it to string
    /// </param>
    /// <returns>Generated JWT string</returns>
    public static string CreateAuthenticationToken(this JwtConfiguration     jwtConfiguration,
                                                   List<Claim>               claims,
                                                   out double                expiresAt,
                                                   string?                   audiencesString  = null,
                                                   double                    expiresInMinutes = 0,
                                                   Action<JwtSecurityToken>? appendData       = null)
    {
        // retrieve the key for signing, preferable using private signing key for more secured
        string signingKey = (jwtConfiguration.PrivateSigningKey ?? jwtConfiguration.SignatureVerificationKey)!;

        // if private and public verification key both available, use asymmetric algorithm.
        var useAsymmetric = !string.IsNullOrEmpty(jwtConfiguration.PrivateSigningKey) &&
                            !string.IsNullOrEmpty(jwtConfiguration.SignatureVerificationKey);

        SecurityKey key = useAsymmetric
                              ? new RsaSecurityKey(RsaCryptoEngine.ImportPemPrivateKey(signingKey))
                              : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        key.CryptoProviderFactory = new CryptoProviderFactory
        {
            CacheSignatureProviders = false
        };

        var securityAlgorithm = useAsymmetric
                                    ? SecurityAlgorithms.RsaSha256
                                    : SecurityAlgorithms.HmacSha256;

        var credentials = new SigningCredentials(key, securityAlgorithm);

        expiresInMinutes = expiresInMinutes == 0
                               ? jwtConfiguration.ExpireAfterMinutes
                               : expiresInMinutes;

        var expiration = expiresInMinutes == 0
                             ? DateTime.UtcNow.AddMinutes(JwtConfiguration.DefaultExpiration)
                             : DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var notBefore = expiration < DateTime.UtcNow
                            ? expiration.AddMinutes(-1)
                            : DateTime.UtcNow;

        List<string> audiences = [];

        IAuthAudiencesContainer? audiencesContainer = null;
        ILogger?                  logger;

        using (var serviceScope = jwtConfiguration.ServiceScopeFactory.CreateScope())
        {
            audiencesContainer = serviceScope.ServiceProvider
                                             .GetService<IAuthAudiencesContainer>();
            logger = serviceScope.ServiceProvider
                                 .GetService<ILogger<JwtConfiguration>>();

            if (string.IsNullOrEmpty(audiencesString))
            {
                var audienceResolvers = serviceScope.ServiceProvider
                                                    .GetServices<ICurrentRequestAudienceResolver>();

                audiences.AddRange(audienceResolvers.SelectMany(x => x.GetAudiences())
                                                    .Distinct());

                logger?.LogInformation("Audiences list: {audiences}", audiences);
            }

            else
            {
                audiences = audiencesString.Split([
                                                      ";"
                                                  ],
                                                  StringSplitOptions.RemoveEmptyEntries)
                                           .ToList();
            }
        }

        var validAudiencesAdded = false;

        foreach (var audience in audiences.Distinct())
        {
            if (audiencesContainer!.IsValidAudience(audience).Result)
            {
                logger?.LogInformation("Audience {audiences} is valid", audience);
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
                validAudiencesAdded = true;
            }
            else
            {
                logger?.LogInformation("Audience {audiences} is invalid", audience);
            }
        }

        // If no valid audiences were added, use the issuer as the default audience
        // This ensures the "aud" claim is always present in the token
        if (!validAudiencesAdded)
        {
            logger?.LogWarning("No valid audiences found. Using issuer '{issuer}' as default audience",
                              jwtConfiguration.Issuer);
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, jwtConfiguration.Issuer!));
        }

        var token = new JwtSecurityToken(jwtConfiguration.Issuer,
                                         claims: claims,
                                         notBefore: notBefore,
                                         expires: expiration,
                                         signingCredentials: credentials);

        appendData?.Invoke(token);

        var writeToken = new JwtSecurityTokenHandler().WriteToken(token);
        expiresAt = expiration.GetUnixTimestamp();

        return writeToken;
    }
}