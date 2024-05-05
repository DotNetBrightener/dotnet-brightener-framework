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