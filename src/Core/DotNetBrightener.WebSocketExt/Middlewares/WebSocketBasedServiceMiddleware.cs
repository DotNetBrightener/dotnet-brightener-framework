using DotNetBrightener.WebSocketExt.Internal;
using DotNetBrightener.WebSocketExt.Messages;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DotNetBrightener.WebSocketExt.Middlewares;

/// <summary>
///     Represents the middleware that enables WebSocket-based communication between the server and the client
/// </summary>
internal class WebSocketBasedServiceMiddleware
{
    private readonly RequestDelegate     _next;
    private readonly WebSocketExtOptions _options;
    private readonly ILogger             _logger;

    public WebSocketBasedServiceMiddleware(RequestDelegate                          next,
                                           ILogger<WebSocketBasedServiceMiddleware> logger,
                                           IOptions<WebSocketExtOptions>            options)
    {
        _next    = next;
        _options = options.Value;
        _logger  = logger;
    }

    public async Task Invoke(HttpContext              httpContext,
                             IHostApplicationLifetime hostApplicationLifetime,
                             IServiceScopeFactory     serviceScopeFactory,
                             IConnectionManager       connectionManager)
    {
        if (httpContext.Request.Path != _options.Path)
        {
            await _next(httpContext);

            return;
        }

        if (!httpContext.WebSockets.IsWebSocketRequest)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            return;
        }

        using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

        await RunAsync(connectionManager,
                       serviceScopeFactory,
                       httpContext,
                       webSocket,
                       hostApplicationLifetime.ApplicationStopping);
    }

    private async Task RunAsync(IConnectionManager   connectionManager,
                                IServiceScopeFactory serviceScopeFactory,
                                HttpContext          httpContext,
                                WebSocket            webSocket,
                                CancellationToken    cancellationToken)
    {
        var query = httpContext.Request.Query;

        string? connectionId = null;
        var     debugMode    = query.ContainsKey(_options.DebugIndicatorQueryName);

        if (query.TryGetValue(_options.ConnectionTokenQueryParamName, out var connectionToken) &&
            !string.IsNullOrEmpty(connectionToken) &&
            connectionManager.TryGetConnection(connectionToken.ToString(), out var connectionInfo) &&
            connectionInfo is not null)
        {
            connectionId = connectionInfo.ConnectionId;

            await connectionManager.AttachConnection(connectionId, webSocket);

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                await connectionManager.AttachUser(connectionId, httpContext.GetCurrentUserId()!.ToString());
            }

            connectionInfo.IsDebugMode = debugMode;
        }

        if (string.IsNullOrEmpty(connectionId))
        {
            connectionId               = Guid.NewGuid().ToString();
            connectionInfo             = await connectionManager.AddConnection(connectionId, webSocket);
            connectionInfo.IsDebugMode = debugMode;
        }


        var notifyConnected = false;

        var bufferSize = _options.SendReceiveBufferSizeInBytes;

        try
        {
            while (webSocket.State.HasFlag(WebSocketState.Open))
            {
                if (!notifyConnected)
                {
                    await NotifyClientConnected(webSocket, connectionId, bufferSize, debugMode, cancellationToken);
                    notifyConnected = true;

                    continue;
                }

                using var               msRequest     = new MemoryStream();
                ArraySegment<byte>      bufferReceive = new(new byte[bufferSize]);
                WebSocketReceiveResult? webSocketMsg;

                do
                {
                    webSocketMsg = await webSocket.ReceiveAsync(bufferReceive, cancellationToken);

                    if (bufferReceive.Array != null)
                    {
                        msRequest.Write(bufferReceive.Array!, bufferReceive.Offset, webSocketMsg.Count);
                    }
                }
                while (!webSocketMsg.CloseStatus.HasValue && !webSocketMsg.EndOfMessage);

                if (webSocketMsg.CloseStatus.HasValue)
                {
                    break;
                }

                RequestMessage? request;

                switch (webSocketMsg.MessageType)
                {
                    case WebSocketMessageType.Text:
                    {
                        var jsonRequest = Encoding.UTF8.GetString(msRequest.ToArray());

                        if (jsonRequest == "ping")
                        {
                            ResponsePingMessage(webSocket, cancellationToken);

                            continue;
                        }

                        try
                        {

                            request = JsonSerializer.Deserialize<RequestMessage>(jsonRequest,
                                                                                 JsonSerializerSettings
                                                                                    .DeserializeOptions)!;
                        }
                        catch
                        {
                            // can't understand the message => ignore
                            continue;
                        }

                        break;
                    }
                    case WebSocketMessageType.Binary:
                        // TODO: Read file upload request if possible

                        request = await msRequest.Decompress(bufferSize);

                        break;
                    default:
                        _logger.LogWarning("Unexpected message type {messageType}", webSocketMsg.MessageType);
                        var responseMessage = ResponseMessage.FromPayload<CommonResponsePayload>(connectionId,
                                                                                                 Guid.NewGuid()
                                                                                                     .ToString(),
                                                                                                 new("Error"),
                                                                                                 $"Unsupported message type {webSocketMsg.MessageType}");
                        webSocket.DeliverMessage(responseMessage,
                                                 bufferSize,
                                                 !debugMode,
                                                 cancellationToken);

                        continue;
                }

                var commandName = request.Action;

                if (commandName == "ping")
                {
                    ResponsePingMessage(webSocket, cancellationToken);

                    continue;
                }

                ProcessRequest(serviceScopeFactory,
                               webSocket,
                               commandName,
                               connectionId,
                               request,
                               bufferSize,
                               cancellationToken,
                               debugMode);
            }
        }
        catch (WebSocketException ex)
        {
            if (ex.Message ==
                "The remote party closed the WebSocket connection without completing the close handshake.")
            {
                _logger.LogWarning(ex, "Socket Connection {connectionId} closed unexpectedly", connectionId);
            }
            else
            {
                _logger.LogError(ex,
                                 "Socket Connection {connectionId} encountered unexpected WebSocket Connection",
                                 connectionId);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex,
                               "Operation cancelled for socket Connection {connectionId} unexpectedly",
                               connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing WebSocket. Socket Connection: {connectionId}", connectionId);

            throw;
        }
        finally
        {
            await connectionManager.RemoveConnection(connectionId);

            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                           "Socket Closed",
                                           cancellationToken);
            }
            catch
            {
                // ignore any error. no need to handle
            }
        }
    }

    private async Task ProcessRequest(IServiceScopeFactory serviceScopeFactory,
                                      WebSocket            webSocket,
                                      string               commandName,
                                      string               connectionId,
                                      RequestMessage       request,
                                      int                  bufferSize,
                                      CancellationToken    cancellationToken,
                                      bool                 debugMode = false)
    {
        ResponseMessage? response;

        await using (var scope = serviceScopeFactory.CreateAsyncScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var commandMetadata = serviceProvider.GetRequiredService<WebSocketCommandMetadata>();

            var handler = commandMetadata.GetHandler(serviceProvider, commandName);

            if (handler is null)
            {
                _logger.LogWarning("No handler found for command {commandName}", commandName);
                await webSocket.DeliverMessage(ResponseMessage.FromPayload<CommonResponsePayload>(connectionId,
                                                                                                  request.Id,
                                                                                                  new(commandName),
                                                                                                  "No handler found for command"),
                                               bufferSize,
                                               !debugMode,
                                               cancellationToken);

                return;
            }

            response = await handler.HandleCommandAsync(request,
                                                        cancellationToken);
        }

        // process the request and get the response

        if (response is not null)
        {
            response.ConnectionId = connectionId;
            response.Id           = request.Id;

            await webSocket.DeliverMessage(response, bufferSize, !debugMode, cancellationToken);
        }
    }

    private static async Task ResponsePingMessage(WebSocket         webSocket,
                                                  CancellationToken cancellationToken)
    {
        await webSocket.SendAsync(new ArraySegment<byte>("pong"u8.ToArray()),
                                  WebSocketMessageType.Text,
                                  true,
                                  cancellationToken);
    }

    private async Task NotifyClientConnected(WebSocket         webSocket,
                                             string            connectionId,
                                             int               bufferSize,
                                             bool              debugMode         = false,
                                             CancellationToken cancellationToken = default)
    {
        var connectedResponse = ResponseMessage.FromPayload<ConnectedNotificationPayload>(connectionId,
                                                                                          Guid.NewGuid().ToString(),
                                                                                          new());
        await webSocket.DeliverMessage(connectedResponse, bufferSize, !debugMode, cancellationToken);
    }
}