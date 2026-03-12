namespace DotNetBrightener.TemplateEngine.Data.SendgridTemplateProvider;

/// <summary>
///     Configuration settings for the Sendgrid Template Provider.
/// </summary>
public class SendgridTemplateProviderSettings
{
    /// <summary>
    ///     The configuration section name in appsettings.json.
    /// </summary>
    public const string ConfigurationSectionName = "SendgridTemplateProvider";

    /// <summary>
    ///     The SendGrid API Key for accessing templates.
    ///     Required scopes: templates.read, templates.create
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     Prefix added to all template names when searching/creating in Sendgrid.
    ///     Used to namespace templates per environment or application.
    ///     Example: "MyApp", "Dev", "Prod"
    /// </summary>
    public string TemplatePrefix { get; set; } = string.Empty;

    /// <summary>
    ///     Cache duration in minutes for template ID lookups.
    ///     Default: 60 minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;

    /// <summary>
    ///     Whether to automatically create templates in Sendgrid if they don't exist during registration.
    ///     Default: true.
    /// </summary>
    public bool AutoCreateTemplates { get; set; } = true;

    /// <summary>
    ///     Whether to fail application startup if template registration encounters an error.
    ///     If false, logs a warning and continues.
    ///     Default: false (log warning, continue startup).
    /// </summary>
    public bool FailOnRegistrationError { get; set; } = false;
}

