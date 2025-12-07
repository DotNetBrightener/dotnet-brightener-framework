using DotNetBrightener.TemplateEngine.Data.Services;
using DotNetBrightener.TemplateEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider.Services;

/// <summary>
///     Sendgrid implementation of the template registration service.
///     Automatically discovers and creates templates in Sendgrid using the naming convention.
/// </summary>
internal class SendgridTemplateRegistrationService : ITemplateRegistrationService, ITemplateStore
{
    private readonly ITemplateContainer                _templateContainer;
    private readonly IEnumerable<ITemplateProvider>    _providers;
    private readonly ISendgridTemplateApiClient        _apiClient;
    private readonly ISendgridTemplateIdCache          _templateIdCache;
    private readonly SendgridTemplateProviderSettings  _settings;
    private readonly ILogger                           _logger;

    private Dictionary<string, string> _existingTemplatesByName = new();

    public SendgridTemplateRegistrationService(
        ITemplateContainer                         templateContainer,
        IEnumerable<ITemplateProvider>             providers,
        ISendgridTemplateApiClient                 apiClient,
        ISendgridTemplateIdCache                   templateIdCache,
        IOptions<SendgridTemplateProviderSettings> settings,
        ILoggerFactory                             loggerFactory)
    {
        _templateContainer = templateContainer;
        _providers         = providers;
        _apiClient         = apiClient;
        _templateIdCache   = templateIdCache;
        _settings          = settings.Value;
        _logger            = loggerFactory.CreateLogger(GetType());
    }

    /// <inheritdoc />
    public async Task RegisterTemplates()
    {
        try
        {
            // Fetch all existing templates from Sendgrid and build a name → ID lookup
            var existingTemplates = await _apiClient.GetAllTemplatesAsync();
            _existingTemplatesByName = existingTemplates.ToDictionary(t => t.Name, t => t.Id);

            _logger.LogInformation("Found {Count} existing templates in Sendgrid", existingTemplates.Count);

            // Let each provider register their templates
            foreach (var templateProvider in _providers)
            {
                await templateProvider.RegisterTemplates(this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Sendgrid template registration");

            if (_settings.FailOnRegistrationError)
            {
                throw;
            }
        }
    }

    /// <inheritdoc />
    public Task RegisterTemplate<TTemplate>() where TTemplate : ITemplateModel
        => RegisterTemplate<TTemplate>(string.Empty, string.Empty);

    /// <inheritdoc />
    public async Task RegisterTemplate<TTemplate>(string templateTitle, string templateContent)
        where TTemplate : ITemplateModel
    {
        var templateType = typeof(TTemplate);
        var typeName     = templateType.Name;
        var fullTypeName = templateType.FullName!;

        // Build the expected Sendgrid template name using the naming convention
        var sendgridTemplateName = $"{_settings.TemplatePrefix}_{typeName}";

        _logger.LogDebug("Registering template type {TemplateType} as Sendgrid template '{SendgridName}'",
                         fullTypeName,
                         sendgridTemplateName);

        try
        {
            // Register the type in the template container
            _templateContainer.RegisterTemplate<TTemplate>();

            // Check if template exists in Sendgrid
            if (_existingTemplatesByName.TryGetValue(sendgridTemplateName, out var existingTemplateId))
            {
                // Template exists - cache the ID
                _templateIdCache.SetTemplateId(fullTypeName, existingTemplateId);
                _logger.LogDebug("Found existing Sendgrid template '{Name}' with ID {TemplateId}",
                                sendgridTemplateName,
                                existingTemplateId);
                return;
            }

            // Template doesn't exist
            if (!_settings.AutoCreateTemplates)
            {
                _logger.LogWarning(
                    "Template '{SendgridName}' does not exist in Sendgrid and AutoCreateTemplates is disabled. " +
                    "Template type {TemplateType} will not be available.",
                    sendgridTemplateName,
                    fullTypeName);
                return;
            }

            // Auto-create the template
            var createdTemplate = await _apiClient.CreateTemplateAsync(sendgridTemplateName);
            _templateIdCache.SetTemplateId(fullTypeName, createdTemplate.Id);

            // Add to local lookup in case another template with same name is registered
            _existingTemplatesByName[sendgridTemplateName] = createdTemplate.Id;

            _logger.LogInformation(
                "Created Sendgrid template '{Name}' with ID {TemplateId} for type {TemplateType}",
                sendgridTemplateName,
                createdTemplate.Id,
                fullTypeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register template type {TemplateType} as '{SendgridName}'",
                            fullTypeName,
                            sendgridTemplateName);

            if (_settings.FailOnRegistrationError)
            {
                throw;
            }
        }
    }
}

