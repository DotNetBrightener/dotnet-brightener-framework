using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using DotNetBrightener.TemplateEngine.Data.Mssql.Entity;
using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Services;

public class SqlServerTemplateStorageService(
    ITemplateRecordDataService               repository,
    ILogger<SqlServerTemplateStorageService> logger)
    : ITemplateStorageService
{
    public void SaveTemplate(string templateType, TemplateModelDto content)
    {
        var typeName = templateType;

        var templateObject = repository.Get(template => template.TemplateType == typeName);

        if (templateObject == null ||
            templateObject.IsDeleted)
        {
            templateObject = new TemplateRecord
            {
                TemplateType = typeName
            };
            repository.Insert(templateObject);
        }

        templateObject.TemplateTitle   = content.TemplateTitle;
        templateObject.TemplateContent = content.TemplateContent;
        repository.Update(templateObject);
    }

    public TemplateModelDto LoadTemplate(string templateModelType)
        => LoadTemplateAsync(templateModelType).Result;

    public async Task<TemplateModelDto> LoadTemplateAsync(string templateModelType)
    {
        var typeName = templateModelType;

        var templateObject = await repository.Fetch(template => template.TemplateType == typeName)
                                             .FirstOrDefaultAsync();

        if (templateObject == null ||
            templateObject.IsDeleted)
        {
            throw new TemplateNotFoundException();
        }

        return new TemplateModelDto
        {
            TemplateContent = templateObject.TemplateContent,
            TemplateType    = templateObject.TemplateType,
            TemplateTitle   = templateObject.TemplateTitle,
        };
    }
}