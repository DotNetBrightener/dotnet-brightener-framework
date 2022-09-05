using Microsoft.AspNetCore.Builder;

namespace DotNetBrightener.Core.Modular.Infrastructure;

public interface IModuleStartup
{
    void OnStartup(IApplicationBuilder applicationBuilder);
}