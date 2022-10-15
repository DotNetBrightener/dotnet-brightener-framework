using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.Modular.Infrastructure;

/// <summary>
///     Declares the dependency services of the modules and registers them to the main collection
/// </summary>
public interface IDependencyRegistra
{
    void ConfigureServices(IServiceCollection serviceCollection);
}