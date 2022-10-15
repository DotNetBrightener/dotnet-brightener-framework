using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using WebEdFramework.Modular.Mvc;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.Views;

/// <summary>
/// Represents the real render engine, which will generate the final HTML output
/// </summary>
public class HandleBarViewRenderEngine : IView
{
    internal readonly ITemplateFileCacheContainer TemplateFileCacheContainer;
    private readonly  string                      _viewInstanceId = Guid.NewGuid().ToString();
    private readonly  ILogger                     _logger;

    public HandleBarViewRenderEngine(ITemplateFileCacheContainer        templateFileCacheContainer,
                                     ILogger<HandleBarViewRenderEngine> logger)
    {
        _logger                    = logger;
        TemplateFileCacheContainer = templateFileCacheContainer;
    }

    public string Path { get; internal set; }

    internal PageRenderContext RenderContext { get; set; }

    public async Task RenderAsync(ViewContext context)
    {
        _logger.LogInformation($"Rendering view id: #{_viewInstanceId}");

        if (!TemplateFileCacheContainer.TryGetTemplate(Path, out var templateCompile))
        {
            var fileContent = await File.ReadAllTextAsync(Path);
            TemplateFileCacheContainer.StoreTemplate(Path, fileContent);                
            TemplateFileCacheContainer.TryGetTemplate(Path, out templateCompile);
        }

        try
        {
            var result = templateCompile(RenderContext);

            RenderContext.RenderedOutput = result;
        }
        catch (HandlebarsRuntimeException exception)
        {
            _logger.LogError(exception, $"Error while compiling template");
            throw;
        }
    }
}