using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.SecuredApi;

internal class SecureApiBuilder
{
    internal SecuredApiHandlerRouter HandlerRouter { get; init; }

    internal IServiceCollection Services { get; init; }
}