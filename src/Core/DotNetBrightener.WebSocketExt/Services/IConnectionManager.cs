using DotNetBrightener.WebSocketExt.Messages;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.WebSockets;

namespace DotNetBrightener.WebSocketExt.Services;

public interface IConnectionManager
{
    Task<ReadOnlyCollection<ConnectionInfo>> GetAllConnections();

    Task AddConnection(string connectionId, ConnectionInfo connectionInfo);

    Task<ConnectionInfo> AddConnection(string connectionId, WebSocket socket);

    Task AttachConnection(string connectionId, WebSocket socket);

    Task AttachUser(string connectionId, string userId);

    Task RemoveConnection(string connectionId);

    bool TryGetConnection(string connectionToken, out ConnectionInfo? connectionInfo);

    Task DeliverMessage(string            connectionId,
                        ResponseMessage   responseMessage,
                        CancellationToken cancellationToken = default);

    Task DeliverMessageToUser(string            userId,
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

    public Task<ReadOnlyCollection<ConnectionInfo>> GetAllConnections()
    {
        return Task.FromResult(new ReadOnlyCollection<ConnectionInfo>(_activeConnections.Values.ToList()));
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

    public Task AttachUser(string connectionId, string userId)
    {
        if (_activeConnections.TryGetValue(connectionId, out var connectionInfo))
        {
            connectionInfo!.UserIdentifier = userId;
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

        await connectionInfo.DeliverMessage(responseMessage,
                                            _options.SendReceiveBufferSizeInBytes,
                                            needCompress: !connectionInfo.IsDebugMode,
                                            cancellationToken);
    }

    public async Task DeliverMessageToUser(string            userId,
                                           ResponseMessage   responseMessage,
                                           CancellationToken cancellationToken = default)
    {
        var tasksList = new List<Task>();

        foreach (var (k, connectionInfo) in _activeConnections)
        {
            if (connectionInfo.UserIdentifier == userId &&
                connectionInfo!.Socket is not null &&
                connectionInfo!.Socket?.State == WebSocketState.Open)
            {
                tasksList.Add(connectionInfo.DeliverMessage(responseMessage,
                                                            _options.SendReceiveBufferSizeInBytes,
                                                            needCompress: !connectionInfo.IsDebugMode,
                                                            cancellationToken));
            }
        }

        await Task.WhenAll(tasksList);
    }

    public async Task DeliverMessageToAllChannels(ResponseMessage   responseMessage,
                                                  CancellationToken cancellationToken = default)
    {
        var tasksList = new List<Task>();
        foreach (var (k, connectionInfo) in _activeConnections)
        {
            if (connectionInfo is null ||
                connectionInfo.Socket is null ||
                connectionInfo.Socket.State != WebSocketState.Open)
                continue;

            tasksList.Add(connectionInfo.DeliverMessage(responseMessage,
                                                        _options.SendReceiveBufferSizeInBytes,
                                                        needCompress: !connectionInfo.IsDebugMode,
                                                        cancellationToken));
        }

        await Task.WhenAll(tasksList);
    }
}