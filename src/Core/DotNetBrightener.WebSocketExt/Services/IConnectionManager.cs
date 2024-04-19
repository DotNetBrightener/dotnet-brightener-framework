using System.Collections.Concurrent;
using System.Net.WebSockets;
using DotNetBrightener.WebSocketExt.Messages;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.WebSocketExt.Services;

public interface IConnectionManager
{
    Task AddConnection(string connectionId, ConnectionInfo connectionInfo);

    Task<ConnectionInfo> AddConnection(string connectionId, WebSocket socket);

    Task AttachConnection(string connectionId, WebSocket socket);

    Task RemoveConnection(string connectionId);

    bool TryGetConnection(string connectionToken, out ConnectionInfo? connectionInfo);

    Task DeliverMessage(string            connectionId,
                        ResponseMessage   responseMessage,
                        CancellationToken cancellationToken = default);

    Task DeliverMessageToAllChannels(ResponseMessage   responseMessage,
                                     CancellationToken cancellationToken = default);
}

public class DefaultConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, ConnectionInfo?> _activeConnections = new();
    private readonly WebSocketExtOptions                           _options;

    public DefaultConnectionManager(IOptions<WebSocketExtOptions> options)
    {
        _options = options.Value;
    }

    public Task AddConnection(string connectionId, ConnectionInfo connectionInfo)
    {
        _activeConnections.TryAdd(connectionId, connectionInfo);

        return Task.CompletedTask;
    }

    public Task<ConnectionInfo> AddConnection(string connectionId, WebSocket socket)
    {
        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = connectionId,
            Socket       = socket
        };

        _activeConnections.TryAdd(connectionId,
                                  connectionInfo);

        return Task.FromResult(connectionInfo);
    }

    public Task AttachConnection(string connectionId, WebSocket socket)
    {
        if (_activeConnections.TryGetValue(connectionId, out var connectionInfo))
        {
            connectionInfo!.Socket = socket;
        }

        return Task.CompletedTask;
    }

    public Task RemoveConnection(string connectionId)
    {
        _activeConnections.Remove(connectionId, out _);

        return Task.CompletedTask;
    }

    public bool TryGetConnection(string connectionToken, out ConnectionInfo? connectionInfo)
    {
        return _activeConnections.TryGetValue(connectionToken, out connectionInfo);
    }

    public async Task DeliverMessage(string            connectionId,
                                     ResponseMessage   responseMessage,
                                     CancellationToken cancellationToken = default)
    {
        if (!TryGetConnection(connectionId, out var connectionInfo) ||
            connectionInfo!.Socket is null ||
            connectionInfo!.Socket?.State != WebSocketState.Open)
            return;

        await connectionInfo.Socket.DeliverMessage(responseMessage,
                                                   _options.SendReceiveBufferSizeInBytes,
                                                   needCompress: !connectionInfo.IsDebugMode,
                                                   cancellationToken);
    }

    public async Task DeliverMessageToAllChannels(ResponseMessage   responseMessage,
                                                  CancellationToken cancellationToken = default)
    {
        foreach (var (k, connectionInfo) in _activeConnections)
        {
            if (connectionInfo is null ||
                connectionInfo.Socket is null ||
                connectionInfo.Socket.State != WebSocketState.Open)
                continue;
            
            await connectionInfo.Socket.DeliverMessage(responseMessage,
                                                       _options.SendReceiveBufferSizeInBytes,
                                                       needCompress: !connectionInfo.IsDebugMode,
                                                       cancellationToken);
        }
    }
}