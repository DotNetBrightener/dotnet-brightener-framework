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
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var serviceProvider     = scope.ServiceProvider;
            var registrationService = serviceProvider.GetRequiredService<ITemplateRegistrationService>();
            await registrationService.RegisterTemplates();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}