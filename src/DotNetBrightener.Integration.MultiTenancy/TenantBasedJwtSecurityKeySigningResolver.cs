using System;
using System.Collections.Generic;
using System.Text;
using DotNetBrightener.Core.Authentication.Configs;
using DotNetBrightener.Core.Authentication.Services;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Integration.MultiTenancy
{
    public class TenantBasedJwtSecurityKeySigningResolver : IJwtSecurityKeySigningResolver
    {
        private readonly IJwtConfigurationAccessor _jwtConfigurationAccessor;


        public TenantBasedJwtSecurityKeySigningResolver(IJwtConfigurationAccessor jwtConfigurationAccessor)
        {
            _jwtConfigurationAccessor = jwtConfigurationAccessor;
        }

        public IEnumerable<SecurityKey> ResolveSigningKey(string                    token,
                                                          SecurityToken             securityToken,
                                                          string                    kid,
                                                          TokenValidationParameters validationParameters)
        {
            List<SecurityKey> keys = new List<SecurityKey>();

            var jwtTokenOptions = _jwtConfigurationAccessor.RetrieveConfig(kid);

            if (jwtTokenOptions != null)
            {
                validationParameters.ValidIssuer   = jwtTokenOptions.Issuer;
                validationParameters.ValidAudience = jwtTokenOptions.Audience;

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenOptions.SigningKey));
                keys.Add(signingKey);
            }
            else
            {
                validationParameters.ValidIssuer   = JwtConfig.JwtDefaultIssuer;
                validationParameters.ValidAudience = "__INVALID__AUDIENCE__";

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                keys.Add(signingKey);
            }

            return keys;
        }
    }
}