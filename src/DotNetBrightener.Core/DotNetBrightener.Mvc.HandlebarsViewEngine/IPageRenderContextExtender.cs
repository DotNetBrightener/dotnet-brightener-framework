using System.Threading.Tasks;

namespace WebEdFramework.Modular.Mvc
{
    public interface IPageRenderContextExtender
    {
        Task ExtendRenderContext(PageRenderContext pageRenderContext);
    }

    public class DefaultPageRenderContextExtender : IPageRenderContextExtender
    {
        public Task ExtendRenderContext(PageRenderContext pageRenderContext)
        {
            return Task.CompletedTask;
        }
    }
}