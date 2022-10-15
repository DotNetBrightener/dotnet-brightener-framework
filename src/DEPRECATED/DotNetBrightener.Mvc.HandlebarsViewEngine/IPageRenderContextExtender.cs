using System.Threading.Tasks;

namespace WebEdFramework.Modular.Mvc;

public interface IPageRenderContextExtender
{
    Task ExtendRenderContext(PageRenderContext pageRenderContext);
}