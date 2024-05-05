using DotNetBrightener.TemplateEngine.Data.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Services;

public interface ITemplateStorageService
{
    void SaveTemplate(string templateType, TemplateModelDto content);

    TemplateModelDto       LoadTemplate(string      templateModelType);

    Task<TemplateModelDto> LoadTemplateAsync(string templateModelType);
}

public class PhysicalTemplateStorageService : ITemplateStorageService
{
    private const    string             TemplatesFolder = "Templates";
    private readonly ITemplateContainer _templateContainer;
    private readonly IFileProvider      _templatesFileProvider;
    private readonly ILogger            _logger;

    public PhysicalTemplateStorageService(IHostEnvironment                        environment,
                                          ITemplateContainer                      templateContainer,
                                          ILogger<PhysicalTemplateStorageService> logger)
    {
        _templateContainer = templateContainer;
        _logger            = logger;

        var templatePath = Path.Combine(environment.ContentRootPath, TemplatesFolder);

        if (!Directory.Exists(templatePath))
        {
            Directory.CreateDirectory(templatePath);
        }

        _templatesFileProvider = new PhysicalFileProvider(templatePath);
    }

    public void SaveTemplate(string templateType, TemplateModelDto content)
    {
        var typeName = templateType;

        var templateObject = _templateContainer.GetAllTemplateTypes()
                                               .FirstOrDefault(type => type.FullName == typeName);

        if (templateObject == null)
        {
            throw new InvalidOperationException($"Template type {typeName} is not supported");
        }

        var templateFile = _templatesFileProvider.GetFileInfo(typeName + ".tpl");

        TplParser.WriteTemplate(content, templateFile);
    }

    public TemplateModelDto LoadTemplate(string templateModelType)
        => LoadTemplateAsync(templateModelType).Result;

    public async Task<TemplateModelDto> LoadTemplateAsync(string templateModelType)
    {
        var typeName = templateModelType;

        var templateType = _templateContainer.GetAllTemplateTypes()
                                             .FirstOrDefault(type => type.FullName == typeName);

        if (templateType == null)
        {
            throw new InvalidOperationException($"Template type {typeName} is not supported");
        }

        var templateFile = _templatesFileProvider.GetFileInfo(typeName + ".tpl");

        var templateModel = TplParser.ReadTemplate(templateFile);

        return templateModel;
    }
}