namespace DotNetBrightener.TemplateEngine.Data.Models;

public class TemplateModelDto
{
    /// <summary>
    ///     The type of the template, e.g. "WelcomeEmail", "PasswordReset", etc.
    /// </summary>
    public string TemplateType { get; set; }

    /// <summary>
    ///     The title of the template, could be used as subject in emails.
    /// </summary>
    public string TemplateTitle { get; set; }

    /// <summary>
    ///     The content of the template, e.g. HTML or plain text.
    /// </summary>
    public string TemplateContent { get; set; }

    /// <summary>
    ///     Contains the editor configuration for the <see cref="TemplateTitle"/>, e.g. when use with an HTML editor.
    /// </summary>
    public string TemplateTitleEditorConfig { get; set; }

    /// <summary>
    ///     Contains the editor configuration for the <see cref="TemplateContent"/>, e.g. when use with an HTML editor.
    /// </summary>
    public string TemplateContentEditorConfig { get; set; }
}