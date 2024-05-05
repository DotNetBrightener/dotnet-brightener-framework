using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data;

internal class TemplateRegistrationStartupTask(
    IServiceScopeFactory                     serviceScopeFactory,
    ILogger<TemplateRegistrationStartupTask> logger,
    IHostApplicationLifetime                 lifetime)
    : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var serviceProvider     = scope.ServiceProvider;
            var registrationService = serviceProvider.GetRequiredService<ITemplateRegistrationService>();
            registrationService.RegisterTemplates().Wait();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}