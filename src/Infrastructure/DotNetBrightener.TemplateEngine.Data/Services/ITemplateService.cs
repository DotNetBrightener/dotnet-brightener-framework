using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotNetBrightener.TemplateEngine.Data.Entity;
using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Exceptions;
using DotNetBrightener.TemplateEngine.Models;
using DotNetBrightener.TemplateEngine.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace DotNetBrightener.TemplateEngine.Data.Services;

public interface ITemplateService 
{
    IEnumerable<TemplateListItemModel> GetAllAvailableTemplates();

    void SaveTemplate(string templateType, TemplateModelDto content);

    TemplateModelDto LoadTemplate<TTemplate>() where TTemplate : ITemplateModel;

    /// <summary>
    /// Loads the template from database and then parse it with the provided model instance
    /// </summary>
    /// <typeparam name="TTemplate">The type of <see cref="instance"/> object</typeparam>
    /// <param name="instance">The model used to parse the template content</param>
    /// <returns></returns>
    TemplateModelDto LoadAndParseTemplate<TTemplate>(TTemplate instance, bool isHtml = true) where TTemplate : ITemplateModel;

    string ParseTemplate(string template, dynamic instance, bool isHtml = true);
}

public class TemplateService : ITemplateService
{
    private readonly ITemplateRecordDataService _repository;
    private readonly IHttpContextAccessor       _httpContextAccessor;
    private readonly ITemplateParserService     _templateParserService;
    private readonly ITemplateContainer         _templateContainer;

    public TemplateService(ITemplateRecordDataService repository,
                           IHttpContextAccessor       httpContextAccessor,
                           ITemplateParserService     templateParserService,
                           ITemplateContainer         templateContainer)
    {
        _repository            = repository;
        _httpContextAccessor   = httpContextAccessor;
        _templateParserService = templateParserService;
        _templateContainer     = templateContainer;
    }

    public IEnumerable<TemplateListItemModel> GetAllAvailableTemplates()
    {
        var allAvailableTemplates = _repository.Fetch(x => !x.IsDeleted)
                                               .ToArray()
                                               .Select(_ => _.TemplateType);

        foreach (var allAvailableTemplate in allAvailableTemplates)
        {
            yield return _templateContainer.GetTemplateInformation(allAvailableTemplate);
        }
    }

    public void SaveTemplate(string templateType, TemplateModelDto content)
    {
        var typeName = templateType;

        var templateObject = _repository.Get(template => template.TemplateType == typeName);

        if (templateObject == null ||
            templateObject.IsDeleted)
        {
            templateObject = new TemplateRecord
            {
                TemplateType = typeName
            };
            _repository.Insert(templateObject);
        }

        templateObject.TemplateTitle   = content.TemplateTitle;
        templateObject.TemplateContent = content.TemplateContent;
        _repository.Update(templateObject);
    }

    public TemplateModelDto LoadTemplate<TTemplate>() where TTemplate : ITemplateModel
    {
        return LoadTemplate(typeof(TTemplate).FullName);
    }

    public TemplateModelDto LoadTemplate(string templateModelType)
    {
        var typeName = templateModelType;

        var templateObject = _repository.Get(template => template.TemplateType == typeName);

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
            Fields          = templateObject.Fields
        };
    }

    public TemplateModelDto LoadAndParseTemplate<TTemplate>(TTemplate instance, bool isHtml = true) where TTemplate : ITemplateModel
    {
        var template = LoadTemplate(instance.GetType().FullName ?? typeof(TTemplate).FullName);

        if (template == null ||
            (string.IsNullOrEmpty(template?.TemplateContent) &&
             string.IsNullOrEmpty(template?.TemplateTitle))
           )
        {
            return null;
        }

        if (instance is BaseTemplateModel baseTemplateModel)
        {
            if (_httpContextAccessor.HttpContext?.Request != null &&
                string.IsNullOrEmpty(baseTemplateModel.SiteUrl))
            {
                var requestUri = new Uri(_httpContextAccessor.HttpContext.Request.GetDisplayUrl());
                baseTemplateModel.SiteUrl = new Uri(requestUri, "/").ToString().Trim('/');
            }
        }

        template.TemplateContent = _templateParserService.ParseTemplate(template.TemplateContent, instance, isHtml);
        template.TemplateTitle = _templateParserService.ParseTemplate(template.TemplateTitle, instance);

        template.TemplateTitle = WebUtility.HtmlDecode(template.TemplateTitle);

        return template;
    }

    public string ParseTemplate(string template, dynamic instance, bool isHtml = true)
    {
        try
        {
            if (_httpContextAccessor.HttpContext?.Request != null &&
                string.IsNullOrEmpty(instance.SiteUrl))
            {
                var requestUri = new Uri(_httpContextAccessor.HttpContext.Request.GetDisplayUrl());
                instance.SiteUrl = new Uri(requestUri, "/").ToString().Trim('/');
            }
        }
        finally
        {
            // trying to set site url failed, just ignore the error.
        }

        return _templateParserService.ParseTemplate(template, instance);
    }
}