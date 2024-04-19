using DotNetBrightener.WebSocketExt.Services;

namespace DotNetBrightener.WebSocketExt.Internal;

internal class WebSocketCommandMetadata : Dictionary<string, Type>
{
    public IWebsocketCommandHandler? GetHandler(IServiceProvider scopedServiceProvider, string commandName)
    {
        if (TryGetValue(commandName, out var type))
        {
            return (IWebsocketCommandHandler)scopedServiceProvider.TryGet(type);
        }

        return null;
    }
}