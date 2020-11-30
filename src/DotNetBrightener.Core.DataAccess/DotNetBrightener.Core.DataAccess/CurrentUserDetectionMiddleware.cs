using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.Core.DataAccess
{
    public class CurrentUserDetectionMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserDetectionMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext      context,
                                 IDataWorkContext dataWorkContext,
                                 IHttpContextAccessor httpContextAccessor)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userName = context.User.FindFirst(CommonUserClaimKeys.UserName)?.Value;
                var userIdString = context.User.FindFirst(CommonUserClaimKeys.UserId)?.Value;

                dataWorkContext.SetContextData(userName, CommonUserConstants.CurrentLoggedInUserName);
                httpContextAccessor.StoreValue(CommonUserConstants.CurrentLoggedInUserName, userName);

                if (long.TryParse(userIdString, out var userId))
                {
                    dataWorkContext.SetContextData(userId, CommonUserConstants.CurrentLoggedInUserId);
                    httpContextAccessor.StoreValue(CommonUserConstants.CurrentLoggedInUserId, userId);
                }
            }

            await _next(context);
        }
    }
}