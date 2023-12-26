using System.Threading.Tasks;
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.WebApp.CommonShared.StartupTasks;

public class TemplateHelperRegistrationTask : IStartupTask
{
    private readonly ITemplateHelperRegistration _templateHelperRegistration;
    private readonly ILogger                     _logger;

    public TemplateHelperRegistrationTask(ITemplateHelperRegistration                templateHelperRegistration,
                                          ILogger<TemplateAutomaticRegistrationTask> logger)
    {
        _templateHelperRegistration = templateHelperRegistration;
        _logger                     = logger;
    }

    public int Order => 1001;

    public async Task Execute()
    {
        _logger.LogInformation($"Registering template helpers...");
        _templateHelperRegistration.RegisterHelpers();
    }
}