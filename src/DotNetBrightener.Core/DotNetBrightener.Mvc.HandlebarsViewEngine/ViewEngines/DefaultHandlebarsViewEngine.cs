using System.Collections.Generic;
using System.Linq;
using DotNetBrightener.Mvc.HandlebarsViewEngine.Views;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine.ViewEngines;

public class DefaultHandlebarsViewEngine : IHandlebarsViewEngine
{
    private readonly HandlebarsMvcViewOptions _options;
    private readonly IWebHostEnvironment      _webHostEnvironment;

    public DefaultHandlebarsViewEngine(IOptions<HandlebarsMvcViewOptions> options,
                                       IWebHostEnvironment                webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        _options            = options.Value;
    }

    public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
    {
        var controllerName = context.GetNormalizedRouteValue(RoutingConstants.ControllerKey);
        var areaName       = context.GetNormalizedRouteValue(RoutingConstants.AreaKey);
        var subAreaName    = context.GetNormalizedRouteValue(RoutingConstants.SubAreaKey);

        var searchedLocations = new List<string>();

        foreach (var location in _options.ViewLocationFormats)
        {
            var view = string.Format(location, viewName, controllerName, areaName, subAreaName);

            IFileInfo viewFile = _webHostEnvironment.ContentRootFileProvider.GetFileInfo(view);

            // indicates that we found the view, and delegate the view to handleBar view engine
            if (viewFile.Exists)
            {
                var handleBarViewWrapper = HandleBarViewWrapper.FromPath(viewFile.PhysicalPath);

                return ViewEngineResult.Found(viewName, handleBarViewWrapper);
            }

            searchedLocations.Add(view);
        }

        return ViewEngineResult.NotFound(viewName, searchedLocations);
    }

    public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
    {
        var applicationRelativePath = PathHelper.GetAbsolutePath(executingFilePath, viewPath);

        if (!PathHelper.IsAbsolutePath(viewPath))
        {
            // Not a path this method can handle.
            return ViewEngineResult.NotFound(applicationRelativePath, Enumerable.Empty<string>());
        }

        return ViewEngineResult.Found(applicationRelativePath,
                                      HandleBarViewWrapper.FromPath(applicationRelativePath));
    }
}