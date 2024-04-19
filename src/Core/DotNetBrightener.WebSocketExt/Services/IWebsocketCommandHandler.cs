using DotNetBrightener.WebSocketExt.Messages;

namespace DotNetBrightener.WebSocketExt.Services;

public interface IWebsocketCommandHandler
{
    Task<ResponseMessage?> HandleCommandAsync(RequestMessage    payload,
                                              CancellationToken cancellationToken);
}