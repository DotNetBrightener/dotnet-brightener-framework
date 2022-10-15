using System.Threading.Tasks;

namespace WebEdFramework.Modular.Mvc;

public class DefaultPageRenderContextExtender : IPageRenderContextExtender
{
    public Task ExtendRenderContext(PageRenderContext pageRenderContext)
    {
        return Task.CompletedTask;
    }
}