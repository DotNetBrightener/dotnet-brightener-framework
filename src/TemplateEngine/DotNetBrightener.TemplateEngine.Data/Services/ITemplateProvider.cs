namespace DotNetBrightener.TemplateEngine.Data.Services;

/// <summary>
///     Represents service that registers the templates to <see cref="ITemplateStore" />
/// </summary>
public interface ITemplateProvider
{
    /// <summary>
    ///    Registers the templates to the specified <paramref name="templateStore" />
    /// </summary>
    /// <param name="templateStore">
    ///     The <see cref="ITemplateStore"/> to register the templates to
    /// </param>
    /// <returns></returns>
    Task RegisterTemplates(ITemplateStore templateStore);
}