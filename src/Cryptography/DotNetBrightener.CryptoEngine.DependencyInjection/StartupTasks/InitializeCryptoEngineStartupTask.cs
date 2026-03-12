using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.CryptoEngine.DependencyInjection.StartupTasks;

internal class InitializeCryptoEngineStartupTask(
    IServiceScopeFactory                       serviceScopeFactory,
    ILogger<InitializeCryptoEngineStartupTask> logger)
    : IHostedService
{
    public int Order => 0;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope        = serviceScopeFactory.CreateScope();
        var       cryptoEngine = scope.ServiceProvider.GetRequiredService<ICryptoEngine>();

        if (cryptoEngine is null)
            return Task.CompletedTask;

        cryptoEngine.Initialize();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}