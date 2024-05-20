using DotNetBrightener.Utils.MessageCompression;
using DotNetBrightener.WebSocketExt.Messages;
using System.Net.WebSockets;

namespace DotNetBrightener.WebSocketExt.Services;

internal static class WebSocketExtensions
{
    /// <summary>
    ///     Delivers the given response message to the client via specified WebSocket connection
    /// </summary>
    /// <param name="webSocket"></param>
    /// <param name="response"></param>
    /// <param name="bufferSize"></param>
    /// <param name="needCompress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task DeliverMessage(this WebSocket    webSocket,
                                            ResponseMessage   response,
                                            int               bufferSize,
                                            bool              needCompress      = true,
                                            CancellationToken cancellationToken = default)
    {
        var responseBytes = await response.ToJsonBytes();

        if (needCompress)
        {
            responseBytes = await responseBytes.Compress();
        }

        using var msResponse     = new MemoryStream(responseBytes);
        var       bufferResponse = new byte[bufferSize];
        var       totalBytesRead = 0;
        var       bytesRead      = 0;

        while ((bytesRead = await msResponse.ReadAsync(bufferResponse, 0, bufferSize, cancellationToken)) > 0)
        {
            ArraySegment<byte> bufferSend = new(bufferResponse, 0, bytesRead);
            totalBytesRead += bytesRead;
            var endOfMessage = totalBytesRead == msResponse.Length;

            var responseMessageType = needCompress ? WebSocketMessageType.Binary : WebSocketMessageType.Text;

            await webSocket.SendAsync(bufferSend,
                                      responseMessageType,
                                      endOfMessage,
                                      cancellationToken);
        }
    }
}