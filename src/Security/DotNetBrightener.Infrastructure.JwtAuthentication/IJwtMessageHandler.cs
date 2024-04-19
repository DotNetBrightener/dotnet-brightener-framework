using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface IJwtMessageHandler
{
    void OnMessageReceived(MessageReceivedContext context);
}

internal class NullJwtMessageHandler : IJwtMessageHandler
{
    public void OnMessageReceived(MessageReceivedContext context)
    {
        
    }
}