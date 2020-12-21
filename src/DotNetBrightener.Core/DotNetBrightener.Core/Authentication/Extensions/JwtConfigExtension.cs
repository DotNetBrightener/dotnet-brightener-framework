using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetBrightener.Core.Authentication.Configs;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Core.Authentication.Extensions
{
    public static class JwtConfigExtension
    {
        public static string CreateAuthenticationToken(this JwtConfig jwtConfig,
                                                       List<Claim>    claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = jwtConfig.ExpireAfterMinutes == 0
                                 ? DateTime.Now.AddMinutes(JwtConfig.DefaultExpiration)
                                 : DateTime.Now.AddMinutes(jwtConfig.ExpireAfterMinutes);

            var token = new JwtSecurityToken(jwtConfig.Issuer,
                                             jwtConfig.Audience,
                                             claims,
                                             expires: expiration,
                                             signingCredentials: credentials,
                                             notBefore: DateTime.Now);

            token.Header.Add("kid", jwtConfig.KID);
            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);
            return writeToken;
        }
    }
}