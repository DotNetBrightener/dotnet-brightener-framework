using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Models;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Services;

public class TemplateRegistrationService : ITemplateRegistrationService, ITemplateStore
{
    private readonly ITemplateContainer             _templateContainer;
    private readonly ITemplateStorageService        _templateStorageService;
    private readonly IEnumerable<ITemplateProvider> _providers;
    private readonly ILogger                        _logger;

    public TemplateRegistrationService(ITemplateContainer             templateContainer,
                                       ITemplateStorageService        templateStorageService,
                                       IEnumerable<ITemplateProvider> providers,
                                       ILoggerFactory                 loggerFactory)
    {
        _templateContainer      = templateContainer;
        _templateStorageService = templateStorageService;
        _providers              = providers;
        _logger                 = loggerFactory.CreateLogger(GetType());
    }

    public virtual async Task RegisterTemplates()
    {
        foreach (var templateProvider in _providers)
        {
            await templateProvider.RegisterTemplates(this);
        }
    }

    async Task ITemplateStore.RegisterTemplate<TTemplate>() =>
        await RegisterTemplate<TTemplate>(string.Empty, string.Empty);

    public virtual async Task RegisterTemplate<TTemplate>(string templateTitle, string templateContent)
        where TTemplate : ITemplateModel
    {
        _templateContainer.RegisterTemplate<TTemplate>();

        if (!string.IsNullOrEmpty(templateTitle) ||
            !string.IsNullOrEmpty(templateContent))
        {
            var templateType = typeof(TTemplate).FullName;

            await _templateStorageService.SaveTemplateAsync(templateType!,
                                                            new TemplateModelDto
                                                            {
                                                                TemplateContent = templateContent,
                                                                TemplateTitle   = templateTitle
                                                            });
        }
    }
}