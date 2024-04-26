using System.Net.WebSockets;
using System.Security.Claims;
using DotNetBrightener.WebSocketExt.Messages;
using DotNetBrightener.WebSocketExt.Services;

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

public static class ConnectionInfoExtensions
{
    public static async Task DeliverMessage(this ConnectionInfo connectionInfo,
                                            ResponseMessage     response,
                                            int                 bufferSize,
                                            bool                needCompress      = true,
                                            CancellationToken   cancellationToken = default)
        => await connectionInfo.Socket!.DeliverMessage(response, bufferSize, needCompress, cancellationToken);
}