using System;
using System.Collections.Generic;
using System.Security.Claims;
using DotNetBrightener.Core.Events;

namespace DotNetBrightener.Core.Security
{
    /// <summary>
    /// Represents event which will be fired when the user is being authorized by the system
    /// </summary>
    public class UserAuthorizingEventMessage: BaseEventMessage
    {
        /// <summary>
        /// The identifier of the user to authorize
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Output claims which will be added to the current context
        /// </summary>
        public List<Claim> Claims { get; set; }

        /// <summary>
        /// Indicates that the user is authenticated or not
        /// </summary>
        public bool IsUserAuthenticated { get; set; }

        /// <summary>
        /// Indicates that the JWT token need to be refreshed
        /// </summary>
        public bool IsTokenNeedRefresh { get; set; }

        /// <summary>
        /// Keep track of when the permissions gets updated.
        /// If the client's tracking time is before when the server got permissions updated, we ask client side to refresh token
        /// </summary>
        public DateTimeOffset? PermissionsUpdated { get; set; }

        /// <summary>
        /// The current <see cref="ClaimsPrincipal"/> object that contains the current user's information
        /// </summary>
        public ClaimsPrincipal ContextUser { get; set; }
    }
}