using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.TemplateEngine.Data.Services;

namespace DotNetBrightener.TemplateEngine.Data.StartupTasks;

public class TemplateRegistrationStartupTask : IStartupTask
{
    private readonly ITemplateRegistrationService _templateRegistrationService;

    public TemplateRegistrationStartupTask(ITemplateRegistrationService templateRegistrationService)
    {
        _templateRegistrationService = templateRegistrationService;
    }

    public async Task Execute()
    {
        await _templateRegistrationService.RegisterTemplates();
    }

    public int Order => 1001;
}