using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Core.Authentication.Services;

public interface IJwtSecurityKeySigningResolver
{
    IEnumerable<SecurityKey> ResolveSigningKey(string                    token,
                                               SecurityToken             securityToken,
                                               string                    kid,
                                               TokenValidationParameters validationParameters);
}