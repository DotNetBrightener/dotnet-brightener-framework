using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.Extensions.Logging;

namespace WebApp.CommonShared.StartupTasks;

public class TemplateAutomaticRegistrationTask : IStartupTask
{
    private readonly IBackgroundTaskScheduler _backgroundTaskScheduler;
    private readonly ILogger                  _logger;

    private static readonly MethodInfo registerTemplateMethod = typeof(ITemplateRegistrationService)
       .GetMethodWithName(nameof(ITemplateRegistrationService.RegisterTemplates));

    public TemplateAutomaticRegistrationTask(ILogger<TemplateAutomaticRegistrationTask> logger,
                                             IBackgroundTaskScheduler                   backgroundTaskScheduler)
    {
        _logger                  = logger;
        _backgroundTaskScheduler = backgroundTaskScheduler;
    }

    public int Order => 1001;

    public async Task Execute()
    {
        _logger.LogInformation($"Enqueuing Registering templates...");

        _backgroundTaskScheduler.EnqueueTask(registerTemplateMethod);

        _logger.LogInformation($"Registering templates added to queue.");
    }
}