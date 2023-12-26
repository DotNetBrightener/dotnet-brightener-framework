using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.TemplateEngine.Data.Services;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.WebApp.CommonShared.StartupTasks;

public class TemplateAutomaticRegistrationTask : IStartupTask
{
    private readonly ITemplateRegistrationService _templateRegistrationService;
    private readonly ILogger                      _logger;

    public TemplateAutomaticRegistrationTask(ITemplateRegistrationService               templateRegistrationService,
                                             ILogger<TemplateAutomaticRegistrationTask> logger)
    {
        _templateRegistrationService = templateRegistrationService;
        _logger                      = logger;
    }

    public int Order => 1001;

    public async Task Execute()
    {
        _logger.LogInformation($"Registering templates...");
        await _templateRegistrationService.RegisterTemplates();
        _logger.LogInformation($"Automatic Template Registration Task Complete.");
    }
}