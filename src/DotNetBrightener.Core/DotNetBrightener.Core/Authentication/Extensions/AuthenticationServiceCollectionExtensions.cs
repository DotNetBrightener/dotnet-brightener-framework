using System.Threading.Tasks;
using DotNetBrightener.Core.Authentication.Configs;
using DotNetBrightener.Core.Authentication.Services;
using DotNetBrightener.Core.Security.ControllerBasePermissionAuthorizationHandler;
using DotNetBrightener.Core.Security.GenericPermissionAuthorizationHandler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.Core.Authentication.Extensions
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddCustomAuthentication(this IServiceCollection servicesCollection)
        {
            var cookieAuthenticationConfig = new CookieAuthenticationOptions();
            servicesCollection.AddSingleton(cookieAuthenticationConfig);
            servicesCollection.AddSingleton<IJwtConfigurationAccessor, JwtConfigurationAccessor>();
            servicesCollection.AddScoped<IJwtSecurityKeySigningResolver, DefaultJwtSecurityKeySigningResolver>();


            servicesCollection.AddScoped<IAuthorizationHandler, PermissionAuthorizeAttributeHandler>();
            servicesCollection.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            servicesCollection.Add(ServiceDescriptor.Scoped<JwtConfig>(provider =>
                                                                       {
                                                                           var resolver = provider.GetService<IJwtConfigurationAccessor>();
                                                                           return resolver.RetrieveConfig();
                                                                       }));

            var authenticationBuilder = servicesCollection.AddAuthentication();
            var serviceProvider       = servicesCollection.BuildServiceProvider();
            var contextAccessor       = serviceProvider.GetService<IHttpContextAccessor>();

            // Add support for cookies
            authenticationBuilder.AddCookie(options =>
                                            {
                                                var requestServices = contextAccessor.HttpContext.RequestServices;

                                                var cookieOptions = requestServices.GetService<CookieAuthenticationOptions>();

                                                if (cookieOptions != null)
                                                {
                                                    options.AccessDeniedPath = cookieOptions.AccessDeniedPath;
                                                    options.LoginPath        = cookieOptions.LoginPath;
                                                    options.LogoutPath       = cookieOptions.LogoutPath;
                                                }
                                            });
            // Add support for JWT
            authenticationBuilder.AddJwtBearer(cfg => ConfigureJwtOptions(cfg, contextAccessor));
            return authenticationBuilder;
        }


        private static void ConfigureJwtOptions(JwtBearerOptions cfg, IHttpContextAccessor contextAccessor)
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken            = false;

            cfg.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                                    {
                                        if (context.Request.Query.TryGetValue("access_token", out var token) ||
                                            context.Request.Query.TryGetValue("authToken", out token))
                                        {
                                            context.Token = token;
                                        }

                                        return Task.CompletedTask;
                                    },

                OnAuthenticationFailed = context =>
                                         {
                                             context.Response.StatusCode  = 401;
                                             context.Response.ContentType = "text/plain";

                                             var responseMessage = "";

                                             if (context.Exception.Message
                                                        .Contains("IDX10501: Signature validation failed."))
                                             {
                                                 responseMessage = $"You are trying to access an unauthorized resource. Please try to log back in.";
                                             }

                                             return context.Response.WriteAsync(responseMessage);
                                         }
            };

            var requestServices = contextAccessor.HttpContext.RequestServices;
            var resolver        = requestServices.GetService<IJwtSecurityKeySigningResolver>();

            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = resolver.ResolveSigningKey,
            };
        }
    }
}