﻿using DotNetBrightener.DataAccess;
using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using DotNetBrightener.TemplateEngine.Data.Mssql.Entity;
using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Models;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Services;

internal class SqlServerTemplateRegistrationService : ITemplateRegistrationService, ITemplateStore
{
    private readonly ITemplateContainer             _templateContainer;
    private readonly IEnumerable<ITemplateProvider> _providers;
    private readonly ITemplateRecordDataService     _templateRecordDataService;
    private readonly ILogger                        _logger;
    private readonly ScopedCurrentUserResolver      _scopedCurrentUserResolver;

    public SqlServerTemplateRegistrationService(ITemplateContainer             templateContainer,
                                                IEnumerable<ITemplateProvider> providers,
                                                ITemplateRecordDataService     templateRecordDataService,
                                                ILoggerFactory                 loggerFactory,
                                                ScopedCurrentUserResolver      scopedCurrentUserResolver)
    {
        _templateContainer         = templateContainer;
        _providers                 = providers;
        _templateRecordDataService = templateRecordDataService;
        _scopedCurrentUserResolver = scopedCurrentUserResolver;
        _logger                    = loggerFactory.CreateLogger(GetType());
    }

    public async Task RegisterTemplates()
    {
        if (!CheckCanProcessTemplate())
            return;

        using (_scopedCurrentUserResolver.StartUseNameScope("Template Registration Service"))
        {
            // mark all templates as deleted, because they'll be registered again after this
            await _templateRecordDataService.DeleteMany(null,
                                                        reason: "Removed during registration");

            foreach (var templateProvider in _providers)
            {
                await templateProvider.RegisterTemplates(this);
            }
        }
    }

    public Task RegisterTemplate<TTemplate>() where TTemplate : ITemplateModel
        => RegisterTemplate<TTemplate>(string.Empty, string.Empty);

    public async Task RegisterTemplate<TTemplate>(string templateTitle, string templateContent)
        where TTemplate : ITemplateModel
    {
        // register the template to the container
        _templateContainer.RegisterTemplate<TTemplate>();

        if (!CheckCanProcessTemplate())
            return;

        var type = typeof(TTemplate).FullName;

        var templateFields = GetTemplateFields<TTemplate>();
        var assemblyName   = typeof(TTemplate).Assembly.GetName().Name;

        _logger.LogInformation("Registering template type {templateType}", type);

        var templatesToUpdate = _templateRecordDataService.Fetch(x => x.TemplateType == type);

        if (!templatesToUpdate.Any())
        {
            _logger.LogInformation("No template type {templateType} registered. Inserting new record...", type);
            var templateRecord = new TemplateRecord
            {
                TemplateType     = type,
                TemplateContent  = templateContent,
                TemplateTitle    = templateTitle,
                CreatedDate      = DateTimeOffset.UtcNow,
                ModifiedDate     = DateTimeOffset.UtcNow,
                Fields           = templateFields,
                FromAssemblyName = assemblyName
            };

            await _templateRecordDataService.InsertAsync(templateRecord);
        }
        else
        {
            await _templateRecordDataService.UpdateMany(r => r.TemplateType == type,
                                                        record => new TemplateRecord
                                                        {
                                                            IsDeleted        = false,
                                                            FieldsString     = string.Join(";", templateFields),
                                                            FromAssemblyName = assemblyName,
                                                            DeletedDate      = null,
                                                            DeletionReason   = null,
                                                            DeletedBy        = null
                                                        });

            if (!string.IsNullOrEmpty(templateContent))
                await _templateRecordDataService.UpdateMany(r => r.TemplateType == type &&
                                                                 string.IsNullOrEmpty(r.TemplateContent),
                                                            record => new TemplateRecord
                                                            {
                                                                TemplateContent = templateContent
                                                            });

            if (!string.IsNullOrEmpty(templateTitle))
                await _templateRecordDataService.UpdateMany(r => r.TemplateType == type &&
                                                                 string.IsNullOrEmpty(r.TemplateTitle),
                                                            record => new TemplateRecord
                                                            {
                                                                TemplateTitle = templateTitle
                                                            });
        }
    }

    protected static List<string> GetTemplateFields<TTemplate>() where TTemplate : ITemplateModel
    {
        var type = typeof(TTemplate);

        var fieldNamesFromType = TemplateFieldsUtils.RetrieveTemplateFields(type);

        return fieldNamesFromType;
    }

    private bool CheckCanProcessTemplate()
    {
        try
        {
            return _templateRecordDataService.Fetch().Count() > -1;
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains($"Cannot create a DbSet for '{nameof(TemplateRecord)}'"))
            {
                _logger.LogInformation("No template record table available in database, ignoring registration.");

                return false;
            }

            _logger.LogWarning(exception, "Error occurs while checking for template record table");

            throw;
        }
    }
}