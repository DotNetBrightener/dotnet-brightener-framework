using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetBrightener.CryptoEngine;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public static class JwtConfigExtension
{
    /// <summary>
    ///     Generates the JWT token for authenticating from given <see cref="claims"/>
    /// </summary>
    /// <param name="jwtConfiguration">The <see cref="JwtConfiguration"/> object</param>
    /// <param name="claims">The claims that form the Identity</param>
    /// <param name="expiresAt">Specifies when the generated token expires</param>
    /// <param name="audiencesString">Specifies the audiences for the generated token</param>
    /// <param name="appendData">
    ///     Performs more actions to the token before generating it to string
    /// </param>
    /// <returns>Generated JWT string</returns>
    public static string CreateAuthenticationToken(this JwtConfiguration    jwtConfiguration,
                                                   List<Claim>              claims,
                                                   out double               expiresAt,
                                                   string                   audiencesString = null,
                                                   Action<JwtSecurityToken> appendData      = null)
    {
        // retrieve the key for signing, preferable using private signing key for more secured
        var signingKey = jwtConfiguration.PrivateSigningKey
                      ?? jwtConfiguration.SignatureVerificationKey;

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

        var expiration = jwtConfiguration.ExpireAfterMinutes == 0
                             ? DateTime.UtcNow.AddMinutes(JwtConfiguration.DefaultExpiration)
                             : DateTime.UtcNow.AddMinutes(jwtConfiguration.ExpireAfterMinutes);

        var notBefore = expiration < DateTime.UtcNow
                            ? expiration.AddMinutes(-1)
                            : DateTime.UtcNow;

        if (!string.IsNullOrEmpty(audiencesString))
        {
            // supporting multiple audiences
            var audiences = audiencesString.Split(new[]
                                                  {
                                                      ";"
                                                  },
                                                  StringSplitOptions.RemoveEmptyEntries);

            foreach (var audience in audiences)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }
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