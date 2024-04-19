using DotNetBrightener.WebSocketExt.Internal;
using DotNetBrightener.WebSocketExt.Messages;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DotNetBrightener.WebSocketExt.Services;

internal static class MessageCompressionService
{
    public static async Task<byte[]> Compress(this ResponseMessage responseMessage)
    {
        var jsonResponseBytes = await responseMessage.ToBytes();

        return await CompressBytes(jsonResponseBytes);
    }

    public static async Task<byte[]> CompressBytes(byte[] jsonResponseBytes)
    {
        using var       msCompress = new MemoryStream();
        await using var gsCompress = new GZipStream(msCompress, CompressionMode.Compress);
        await gsCompress.WriteAsync(jsonResponseBytes, 0, jsonResponseBytes.Length);

        gsCompress.Close();
        var result = msCompress.ToArray();

        return result;
    }

    public static async Task<byte[]> ToBytes(this ResponseMessage responseMessage)
    {
        var jsonResponse      = JsonSerializer.Serialize(responseMessage, JsonSerializerSettings.SerializeOptions);
        var jsonResponseBytes = Encoding.UTF8.GetBytes(jsonResponse);

        return jsonResponseBytes;
    }

    public static async Task<RequestMessage> Decompress(this MemoryStream msRequest, int bufferSize)
    {
        msRequest.Seek(0, SeekOrigin.Begin);
        var             bufferDecompress    = new byte[bufferSize];
        await using var gsDecompress        = new GZipStream(msRequest, CompressionMode.Decompress);
        using var       msDecompress        = new MemoryStream();
        var             bytesReadDecompress = 0;

        while ((bytesReadDecompress = await gsDecompress.ReadAsync(bufferDecompress, 0, bufferDecompress.Length)) > 0)
        {
            await msDecompress.WriteAsync(bufferDecompress, 0, bytesReadDecompress);
        }

        msDecompress.Seek(0, SeekOrigin.Begin);
        using var srDecompress = new StreamReader(msDecompress, Encoding.UTF8);
        var       jsonRequest  = await srDecompress.ReadToEndAsync();
        var result =
            JsonSerializer.Deserialize<RequestMessage>(jsonRequest, JsonSerializerSettings.DeserializeOptions)!;

        return result;
    }

    public static async Task DeliverMessage(this WebSocket    webSocket,
                                            ResponseMessage   response,
                                            int               bufferSize,
                                            bool              needCompress      = true,
                                            CancellationToken cancellationToken = default)
    {
        var responseBytes = await response.ToBytes();

        if (needCompress)
        {
            responseBytes = await CompressBytes(responseBytes);
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