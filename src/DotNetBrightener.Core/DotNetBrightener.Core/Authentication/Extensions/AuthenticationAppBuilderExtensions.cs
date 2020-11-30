using System;
using Microsoft.AspNetCore.Builder;

namespace DotNetBrightener.Core.Authentication.Extensions
{
    /// <summary>
    /// Extension methods to add authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class AuthenticationAppBuilderExtensions
    {
        /// <summary>
        ///     Adds the <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationMiddleware" /> to the specified
        ///     <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />,
        ///     which enables authentication capabilities.
        /// </summary>
        /// <param name="app">
        ///     The <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /> to add the middleware to.
        /// </param>
        public static void UseCustomAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            app.Use(async (context, next) =>
                    {
                        // support for websocket authentication
                        if (context.Request.Query.TryGetValue("token", out var token) ||
                            context.Request.Query.TryGetValue("authToken", out token) ||
                            context.Request.Query.TryGetValue("access_token", out token))
                        {
                            if (!context.Request.Headers.ContainsKey("Authorization"))
                            {
                                context.Request.Headers.Add("Authorization", new[] {"Bearer " + token});
                            }
                        }

                        await next();
                    });
            
            app.UseCookiePolicy();
            // let the system authenticate the user first
            app.UseMiddleware<OverrideAuthenticationMiddleware>();
        }

        public static void UseCustomAuthorization(this IApplicationBuilder app)
        {
            // once we have the user then we perform the authorization
            app.UseMiddleware<OverrideAuthorizationMiddleware>();
        }
    }
}