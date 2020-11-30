using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetBrightener.Core.Events;
using DotNetBrightener.Core.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Authentication
{
    public class OverrideAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger         _logger;

        public OverrideAuthorizationMiddleware(RequestDelegate                          next,
                                               ILogger<OverrideAuthorizationMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext          httpContext,
                                 IEventPublisher      eventPublisher,
                                 IHttpContextAccessor httpContextAccessor)
        {
            if (httpContext.User == null)
            {
                await _next.Invoke(httpContext);
                return;
            }

            var userIdClaim = httpContext.User.FindFirst(_ => _.Type == CommonUserClaimKeys.UserId);
            var userIdString = userIdClaim?.Value;

            // if cannot get the user id, it means nothing to assign the context.User
            if (long.TryParse(userIdString, out var userId))
            {
                var userPermissionAuthorizingContext = new UserAuthorizingEventMessage
                {
                    IsUserAuthenticated = httpContext.User.Identity.IsAuthenticated,
                    ContextUser = httpContext.User,
                    UserId      = userId,
                    Claims      = new List<Claim>()
                };

                await eventPublisher.Publish(userPermissionAuthorizingContext);

                httpContextAccessor.StoreValue(userPermissionAuthorizingContext);

                if (!userPermissionAuthorizingContext.IsUserAuthenticated)
                {
                    httpContext.User = null;
                    await _next.Invoke(httpContext);
                    return;
                }

                if (userPermissionAuthorizingContext.Claims.Any())
                {
                    var claimIdentity = new ClaimsIdentity(userPermissionAuthorizingContext.Claims);
                    httpContext.User.AddIdentity(claimIdentity);
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}