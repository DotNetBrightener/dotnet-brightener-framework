using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.CryptoEngine.DependencyInjection.StartupTasks;

internal class InitializeCryptoEngineStartupTask : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger              _logger;

    public InitializeCryptoEngineStartupTask(IServiceScopeFactory                       serviceScopeFactory,
                                             ILogger<InitializeCryptoEngineStartupTask> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = logger;
    }

    public int Order => 0;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope        = _serviceScopeFactory.CreateScope();
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