using System.Threading.Tasks;

namespace DotNetBrightener.TemplateEngine.Services;

public interface ITemplateProvider 
{
    Task RegisterTemplates(ITemplateStore templateStore);
}

public class DefaultTemplateProvider : ITemplateProvider
{
    public Task RegisterTemplates(ITemplateStore templateStore)
    {
        return Task.CompletedTask;

    }
}