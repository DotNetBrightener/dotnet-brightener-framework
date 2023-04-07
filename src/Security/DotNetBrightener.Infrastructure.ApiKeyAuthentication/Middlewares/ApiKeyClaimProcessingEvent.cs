using System.Collections.Generic;
using System.Security.Claims;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Models;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Middlewares;

public class ApiKeyClaimProcessingEvent : IEventMessage
{
    public List<Claim> Claims { get; set; }
    public ApiKey      ApiKey { get; set; }
}