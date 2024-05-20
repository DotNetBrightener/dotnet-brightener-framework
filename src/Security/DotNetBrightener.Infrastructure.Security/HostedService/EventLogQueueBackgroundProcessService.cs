#nullable enable
using DotNetBrightener.Infrastructure.Security.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetBrightener.Infrastructure.Security.HostedService;

public class PermissionLoaderAndValidator(IServiceScopeFactory serviceScopeFactory) : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken stoppingToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var permissionContainer = scope.ServiceProvider.GetService<IPermissionsContainer>();

            permissionContainer?.LoadAndValidatePermissions();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}