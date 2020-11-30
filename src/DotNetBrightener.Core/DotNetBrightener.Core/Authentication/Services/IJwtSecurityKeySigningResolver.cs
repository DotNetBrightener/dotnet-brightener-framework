using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Core.Authentication.Services
{
    public interface IJwtSecurityKeySigningResolver
    {
        IEnumerable<SecurityKey> ResolveSigningKey(string                    token,
                                                   SecurityToken             securityToken,
                                                   string                    kid,
                                                   TokenValidationParameters validationParameters);
    }

    public class DefaultJwtSecurityKeySigningResolver : IJwtSecurityKeySigningResolver
    {
        private readonly IJwtConfigurationAccessor _jwtConfigurationAccessor;

        public DefaultJwtSecurityKeySigningResolver(IJwtConfigurationAccessor jwtConfigurationAccessor)
        {
            _jwtConfigurationAccessor = jwtConfigurationAccessor;
        }

        public IEnumerable<SecurityKey> ResolveSigningKey(string                    token,
                                                          SecurityToken             securityToken,
                                                          string                    kid,
                                                          TokenValidationParameters validationParameters)
        {
            var jwtTokenOptions = _jwtConfigurationAccessor.RetrieveConfig();

            validationParameters.ValidIssuer   = jwtTokenOptions.Issuer;
            validationParameters.ValidAudience = jwtTokenOptions.Audience;

            List<SecurityKey> keys = new List<SecurityKey>();

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenOptions.SigningKey));
            keys.Add(signingKey);

            return keys;
        }
    }
}