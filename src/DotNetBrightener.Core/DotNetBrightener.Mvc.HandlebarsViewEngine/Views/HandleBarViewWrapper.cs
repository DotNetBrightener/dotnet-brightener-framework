using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebEdFramework.Modular.Mvc;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.Views
{
    /// <summary>
    /// Represents the wrapper for HandleBar View Engine, which will delegate the rendering process to the actual render engine
    /// </summary>
    public class HandleBarViewWrapper : IView
    {
        public string Path { get; private set; }

        public string ModuleRootPath { get; internal set; }

        /// <summary>
        /// Called by the .Net MVC engine, to render the output HTML from the current context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task RenderAsync(ViewContext context)
        {
            var renderContext = new PageRenderContext(context);

            string finalResult = await RenderFromViewPath(context, Path, renderContext);

            if (finalResult == null)
                throw new InvalidOperationException($"Failed to execute the render job for the current context");

            await context.Writer.WriteAsync(finalResult);
        }

        private async Task<string> RenderFromViewPath(ViewContext context,
                                                      string viewPath,
                                                      PageRenderContext renderContext,
                                                      bool isMasterPage = false)
        {
            var serviceProvider = context.HttpContext.RequestServices;
            var view = serviceProvider.GetService<HandleBarViewRenderEngine>();

            if (!renderContext.PageRenderContextExtendingCalled)
            {
                var extenders = serviceProvider.GetServices<IPageRenderContextExtender>()
                                               .Where(_ => _.GetType() != typeof(DefaultPageRenderContextExtender));

                foreach (var pageRenderContextExtender in extenders)
                {
                    await pageRenderContextExtender.ExtendRenderContext(renderContext);
                }

                renderContext.PageRenderContextExtendingCalled = true;
            }

            if (view != null)
            {
                view.Path = !isMasterPage ? Path : ResolveMasterPagePath(viewPath, context);

                renderContext.CurrentPagePath = view.Path;

                view.RenderContext = renderContext;

                await view.RenderAsync(context);

                var result = view.RenderContext.RenderedOutput;

                // clean up the render context before continue render the nested view
                renderContext.CurrentPageModule = null;
                renderContext.CurrentPageModulePath = null;
                renderContext.CurrentPagePath = null;

                if (!string.IsNullOrEmpty(renderContext.PendingMasterPageTemplatePath))
                {
                    renderContext.NextPageLevel();
                    var masterPagePath = renderContext.PendingMasterPageTemplatePath;
                    renderContext.PendingMasterPageTemplatePath = string.Empty;
                    result = await RenderFromViewPath(context, masterPagePath, renderContext, true);
                }

                return result;
            }

            return null;
        }

        private string ResolveMasterPagePath(string masterPagePath, ViewContext context)
        {
            if (masterPagePath == Path)
                return Path;

            if (masterPagePath.StartsWith("/"))
            {
                var hostEnvironment = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
                return hostEnvironment.ContentRootPath
                                      .CombinePath("Views")
                                      .GetRelativePath(masterPagePath);
            }

            return Path.GetRelativePath(masterPagePath);
        }

        public static HandleBarViewWrapper FromPath(string path)
        {
            return new HandleBarViewWrapper
            {
                Path = path
            };
        }
    }
}