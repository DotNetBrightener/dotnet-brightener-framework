using System.Threading.Tasks;

namespace DotNetBrightener.TemplateEngine.Data.Services;

/// <summary>
///     Represents service that registers the templates to <see cref="ITemplateStore" />
/// </summary>
public interface ITemplateProvider 
{
    /// <summary>
    ///    Registers the templates to <see cref="ITemplateStore" />
    /// </summary>
    /// <param name="templateStore"></param>
    /// <returns></returns>
    Task RegisterTemplates(ITemplateStore templateStore);
}

public class DefaultTemplateProvider : ITemplateProvider
{
    public Task RegisterTemplates(ITemplateStore templateStore)
    {
        return Task.CompletedTask;

    }
}