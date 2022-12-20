using System.Threading.Tasks;

namespace DotNetBrightener.TemplateEngine.Services
{
    public interface ITemplateStore
    {
        Task RegisterTemplate<TTemplate>() where TTemplate : ITemplateModel;
        
        Task RegisterTemplate<TTemplate>(string templateTitle, string templateContent) 
            where TTemplate : ITemplateModel;
    }
}