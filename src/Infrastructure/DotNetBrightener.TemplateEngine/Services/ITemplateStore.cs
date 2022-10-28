using System.Threading.Tasks;

namespace DotNetBrightener.TemplateEngine.Services
{
    public interface ITemplateStore
    {
        Task RegisterTemplate<TTemplate>() where TTemplate : ITemplateModel;
    }
}