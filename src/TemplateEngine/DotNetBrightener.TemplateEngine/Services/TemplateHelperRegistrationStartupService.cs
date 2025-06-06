﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Services;

internal class TemplateHelperRegistrationStartupService(
    IServiceScopeFactory                              serviceScopeFactory,
    ILogger<TemplateHelperRegistrationStartupService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var serviceScope = serviceScopeFactory.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            var templateHelperRegistration = serviceProvider.GetRequiredService<ITemplateHelperRegistration>();

            templateHelperRegistration.RegisterHelpers();
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}