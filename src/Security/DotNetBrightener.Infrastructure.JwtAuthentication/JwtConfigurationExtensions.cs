using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetBrightener.CryptoEngine;
using Microsoft.Extensions.DependencyInjection;
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

        IAuthAudiencesContainer audiencesContainer = null;

        using (var serviceScope = jwtConfiguration.ServiceScopeFactory.CreateScope())
        {
            audiencesContainer = serviceScope.ServiceProvider
                                             .GetService<IAuthAudiencesContainer>();

            if (string.IsNullOrEmpty(audiencesString))
            {
                var audienceResolvers = serviceScope.ServiceProvider
                                                    .GetServices<ICurrentRequestAudienceResolver>();

                foreach (var getAudience in audienceResolvers)
                {
                    audiences.AddRange(getAudience.GetAudiences());
                }
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

        foreach (var audience in audiences.Distinct())
        {
            if (audiencesContainer.IsValidAudience(audience).Result)
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
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