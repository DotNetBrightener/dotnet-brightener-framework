using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Models;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Services;

public class DefaultTemplateService : ITemplateService
{
    protected readonly IHttpContextAccessor    HttpContextAccessor;
    protected readonly ITemplateParserService  TemplateParserService;
    protected readonly ITemplateContainer      TemplateContainer;
    protected readonly ITemplateStorageService TemplateStorageService;
    protected readonly ILogger                 Logger;

    public DefaultTemplateService(IHttpContextAccessor    httpContextAccessor,
                                  ITemplateParserService  templateParserService,
                                  ITemplateContainer      templateContainer,
                                  ITemplateStorageService templateStorageService,
                                  ILoggerFactory          loggerFactory)
    {
        HttpContextAccessor    = httpContextAccessor;
        TemplateParserService  = templateParserService;
        TemplateContainer      = templateContainer;
        TemplateStorageService = templateStorageService;
        Logger                 = loggerFactory.CreateLogger(GetType());
    }

    public List<TemplateListItemModel> GetAllAvailableTemplates()
    {
        var templateTypes = TemplateContainer.GetAllTemplateTypes()
                                             .Select(type => type.FullName)
                                             .ToArray();

        return templateTypes.Select(TemplateContainer.GetTemplateInformation)
                            .ToList();
    }

    public virtual void SaveTemplate(string templateType, TemplateModelDto content)
        => SaveTemplateAsync(templateType, content).Wait();

    public virtual async Task SaveTemplateAsync(string templateType, TemplateModelDto content) =>
        await TemplateStorageService.SaveTemplateAsync(templateType, content);

    public virtual TemplateModelDto LoadTemplate<TTemplate>() where TTemplate : ITemplateModel
        => LoadTemplateAsync<TTemplate>().Result;

    public virtual async Task<TemplateModelDto> LoadTemplateAsync<TTemplate>() where TTemplate : ITemplateModel
        => await TemplateStorageService.LoadTemplateAsync(typeof(TTemplate).FullName);

    public TemplateModelDto LoadTemplate(string templateModelType)
        => LoadTemplateAsync(templateModelType).Result;

    public async Task<TemplateModelDto> LoadTemplateAsync(string templateModelType)
        => await TemplateStorageService.LoadTemplateAsync(templateModelType);

    public virtual TemplateModelDto LoadAndParseTemplate<TTemplate>(TTemplate instance, bool isHtml = true)
        where TTemplate : ITemplateModel
        => LoadAndParseTemplateAsync(instance, isHtml).Result;

    public virtual async Task<TemplateModelDto> LoadAndParseTemplateAsync<TTemplate>(TTemplate instance,
                                                                                     bool      isHtml = true)
        where TTemplate : ITemplateModel
    {
        var template = await TemplateStorageService.LoadTemplateAsync(instance.GetType().FullName ??
                                                                      typeof(TTemplate).FullName);

        if (template == null ||
            (string.IsNullOrEmpty(template.TemplateContent) &&
             string.IsNullOrEmpty(template.TemplateTitle))
           )
        {
            return null;
        }

        var (templateContent, templateTitle) = await (ParseTemplateAsync(template.TemplateContent,
                                                                         instance,
                                                                         isHtml),
                                                      ParseTemplateAsync(template.TemplateTitle,
                                                                         instance,
                                                                         false));
        template.TemplateContent = templateContent;
        template.TemplateTitle   = templateTitle;

        return template;
    }


    public virtual async Task<string> ParseTemplateAsync(string  template,
                                                         dynamic instance,
                                                         bool    isHtml = true)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        try
        {
            if (instance is BaseTemplateModel baseTemplateModel &&
                HttpContextAccessor.HttpContext?.Request != null &&
                string.IsNullOrEmpty(baseTemplateModel.SiteUrl))
            {
                var requestUri = new Uri(HttpContextAccessor.HttpContext.Request.GetDisplayUrl());
                baseTemplateModel.SiteUrl = new Uri(requestUri, "/").ToString().Trim('/');
            }
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Unable to set site url for the template");
        }

        return await TemplateParserService.ParseTemplateAsync(template, instance, isHtml);
    }

    public virtual string ParseTemplate(string template, dynamic instance, bool isHtml = true)
        => ParseTemplateAsync(template, instance, isHtml).Result;
}