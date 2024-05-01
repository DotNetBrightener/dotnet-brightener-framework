using DotNetBrightener.TemplateEngine.Data.Entity;
using DotNetBrightener.TemplateEngine.Models;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Data.Services;

/// <summary>
///     Represents the service that is responsible for registering templates
/// </summary>
public interface ITemplateRegistrationService 
{
    /// <summary>
    ///     Automatically detects and registers templates via <see cref="ITemplateProvider" />
    /// </summary>
    Task RegisterTemplates();
}

public class TemplateRegistrationService : ITemplateRegistrationService, ITemplateStore
{
    private readonly ITemplateContainer             _templateContainer;
    private readonly IEnumerable<ITemplateProvider> _providers;
    private readonly ITemplateRecordDataService     _templateRecordDataService;
    private readonly ILogger                        _logger;

    public TemplateRegistrationService(ITemplateContainer                   templateContainer,
                                       IEnumerable<ITemplateProvider>       providers,
                                       ITemplateRecordDataService           templateRecordDataService,
                                       ILogger<TemplateRegistrationService> logger)
    {
        _templateContainer         = templateContainer;
        _providers                 = providers;
        _templateRecordDataService = templateRecordDataService;
        _logger                    = logger;
    }

    public async Task RegisterTemplates()
    {
        if (!CheckCanProcessTemplate())
            return;

        // mark all templates from the assembly of the provider as deleted,
        // as they'll be re-registered again
        var allAssembliesNames = _providers
                                .Select(templateProvider => templateProvider.GetType().Assembly.GetName().Name)
                                .ToArray();

        await _templateRecordDataService.UpdateMany(record => allAssembliesNames.Contains(record.FromAssemblyName),
                                                    model => new TemplateRecord
                                                    {
                                                        IsDeleted      = true,
                                                        DeletedDate    = DateTimeOffset.UtcNow,
                                                        DeletionReason = "Removed during registration"
                                                    });

        foreach (var templateProvider in _providers)
        {
            await templateProvider.RegisterTemplates(this);
        }
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

    async Task ITemplateStore.RegisterTemplate<TTemplate>() =>
        await RegisterTemplate<TTemplate>(string.Empty, string.Empty);

    public async Task RegisterTemplate<TTemplate>(string templateTitle, string templateContent)
        where TTemplate : ITemplateModel
    {
        if (!CheckCanProcessTemplate())
            return;

        _templateContainer.RegisterTemplate<TTemplate>();

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
                                                            DeletionReason   = null
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

    private static List<string> GetTemplateFields<TTemplate>() where TTemplate : ITemplateModel
    {
        var type = typeof(TTemplate);

        var fieldNamesFromType = GetFieldNamesFromType(type);

        return fieldNamesFromType;
    }

    private static List<string> GetFieldNamesFromType(Type         type,
                                                      string       name          = "",
                                                      List<string> recursiveList = null,
                                                      int          level         = 0)
    {
        if (recursiveList == null)
            recursiveList = new List<string>();

        if (level > 3)
            return recursiveList;

        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            recursiveList.Add($"{name}.{property.Name}".Trim('.'));

            if (property.PropertyType.IsClass &&
                property.PropertyType.IsNotSystemType())
            {
                GetFieldNamesFromType(property.PropertyType,
                                      $"{name}.{property.Name}".Trim('.'),
                                      recursiveList,
                                      level + 1);
            }
        }

        return recursiveList;
    }
}