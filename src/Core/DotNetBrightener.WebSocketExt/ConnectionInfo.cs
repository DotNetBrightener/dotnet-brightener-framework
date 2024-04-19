using System.Net.WebSockets;
using System.Security.Claims;

namespace DotNetBrightener.WebSocketExt;

public class ConnectionInfo
{
    public string ConnectionId { get; set; }

    public string? UserIdentifier { get; set; }

    public string? AuthToken { get; set; }

    internal WebSocket? Socket { get; set; }

    public ClaimsPrincipal? UserPrincipal { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsDebugMode { get; set; }
}