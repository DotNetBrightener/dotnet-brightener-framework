using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.CommonShared.StartupTasks;

public class TemplateAutomaticRegistrationTask : IStartupTask, IDependency
{
    private readonly ITemplateRegistrationService _templateRegistrationService;
    private readonly ITemplateHelperRegistration  _templateHelperRegistration;
    private readonly ILogger                      _logger;

    public TemplateAutomaticRegistrationTask(ITemplateRegistrationService               templateRegistrationService,
                                             ITemplateHelperRegistration                templateHelperRegistration,
                                             ILogger<TemplateAutomaticRegistrationTask> logger)
    {
        _templateRegistrationService = templateRegistrationService;
        _templateHelperRegistration  = templateHelperRegistration;
        _logger                      = logger;
    }

    public int Order => 1001;

    public async Task Execute()
    {
        _logger.LogInformation($"Registering template helpers...");
        _templateHelperRegistration.RegisterHelpers();

        _logger.LogInformation($"Registering templates...");
        await _templateRegistrationService.RegisterTemplates();
        _logger.LogInformation($"Automatic Template Registration Task Complete.");
    }
}