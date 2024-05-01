using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Services;

internal class TemplateHelperRegistrationStartupService: IHostedService
{
    private readonly IServiceScopeFactory                              _serviceScopeFactory;
    private readonly ILogger<TemplateHelperRegistrationStartupService> _logger;

    public TemplateHelperRegistrationStartupService(IServiceScopeFactory                       serviceScopeFactory,
                                                    ILogger<TemplateHelperRegistrationStartupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var serviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;

        var templateHelperRegistration = serviceProvider.GetRequiredService<ITemplateHelperRegistration>();

        templateHelperRegistration.RegisterHelpers();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }
}
