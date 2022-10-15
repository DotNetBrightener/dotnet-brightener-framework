using DotNetBrightener.Mvc.HandlebarsViewEngine;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace WebEdFramework.Modular.Mvc;

/// <summary>
/// Represents the context of rendering a page from current Http request
/// </summary>
public class PageRenderContext: DynamicObject
{
    /// <summary>
    /// The route values of the current HttpContext request
    /// </summary>
    public RouteValueDictionary RouteValues { get; set; }

    /// <summary>
    /// The model of the page
    /// </summary>
    public dynamic Model { get; set; }

    /// <summary>
    /// The model state of the page which contains the validation information
    /// </summary>
    public ModelStateDictionary ModelState { get; set; }

    /// <summary>
    /// The module which is handling the request
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// The controller which is handling the request
    /// </summary>
    public string Controller { get; set; }

    /// <summary>
    /// The area in the module which is handling the request
    /// </summary>
    public string SubArea { get; set; }

    /// <summary>
    /// The resources that are needed for the current page
    /// </summary>
    internal PageRenderResource PageResources { get; set; } = new PageRenderResource();

    /// <summary>
    /// The current view context
    /// </summary>
    public ViewContext ViewContext { get; }

    /// <summary>
    /// The rendered output
    /// </summary>
    public string RenderedOutput { get; set; }

    /// <summary>
    /// The path to the master page template that is requested by the current template
    /// </summary>
    public string PendingMasterPageTemplatePath { get; set; }

    /// <summary>
    /// Retrieves the module which the current template is loaded from
    /// </summary>
    public string CurrentPageModule { get; internal set; }

    /// <summary>
    /// Retrieves the path to the module which the current template is loaded from
    /// </summary>
    public string CurrentPageModulePath { get; internal set; }

    /// <summary>
    /// The path to current page's template
    /// </summary>
    public string CurrentPagePath { get; set; }

    /// <summary>
    /// Indicates whether some services have extended the page render context for their purposes. 
    /// </summary>
    /// <remarks>
    /// Once the context is extended, the extending methods will not be recalled
    /// </remarks>
    internal bool PageRenderContextExtendingCalled { get; set; }

    /// <summary>
    ///     The dynamic view bag
    /// </summary>
    public dynamic ViewBag { get; internal set; }

    public PageRenderContext()
    {
            
    }

    public PageRenderContext(ViewContext viewContext)
    {
        ViewContext = viewContext;
        Controller  = viewContext.GetNormalizedRouteValue(RoutingConstants.ControllerKey);
        Module      = viewContext.GetNormalizedRouteValue(RoutingConstants.AreaKey);
        SubArea     = viewContext.GetNormalizedRouteValue(RoutingConstants.SubAreaKey);
        RouteValues = viewContext.RouteData.Values;
        Model       = viewContext.ViewData.Model;
        ModelState  = viewContext.ViewData.ModelState;
        ViewBag     = viewContext.ViewBag;
    }

    public void NextPageLevel()
    {
        PageResources.NewPageLevel();
    }

    public IEnumerable<string> GetScriptUrls()
    {
        return PageResources.GetOrderedScriptUrls();
    }

    public IEnumerable<string> GetScriptContents()
    {
        return PageResources.GetOrderedScriptContents();
    }

    public IEnumerable<string> GetStyleUrls()
    {
        return PageResources.GetOrderedStyleUrls();
    }

    /// <summary>
    /// Includes the script url in the current page level
    /// </summary>
    /// <param name="scriptUrl">The url to the script to include</param>
    public void IncludeScript(string scriptUrl)
    {
        PageResources.AddScriptUrl(scriptUrl);
    }

    /// <summary>
    /// Includes a script url at top of the current page level
    /// </summary>
    /// <param name="scriptUrl">The url to the script to include, which is required by the dependent scripts</param>
    public void RequireScript(string scriptUrl)
    {
        PageResources.AddRequireScript(scriptUrl);
    }

    /// <summary>
    /// Includes a content of script to the current page level
    /// </summary>
    /// <param name="scriptContent">The content of script to add</param>
    public void AddScriptContent(string scriptContent)
    {
        PageResources.AddScript(scriptContent);
    }

    /// <summary>
    /// Includes a style url in the current page level
    /// </summary>
    /// <param name="styleUrl"></param>
    public void AddStyleUrl(string styleUrl)
    {
        PageResources.AddStyleUrl(styleUrl);
    }

    private readonly ConcurrentDictionary<string, object> _dynamicData = new ConcurrentDictionary<string, object>();

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return _dynamicData.Keys;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (binder == null)
        {
            throw new ArgumentNullException(nameof(binder));
        }

        var propertyInfo = GetType().GetProperty(binder.Name);
        if (propertyInfo != null)
        {
            result = propertyInfo.GetValue(this);
            return true;
        }

        _dynamicData.TryGetValue(binder.Name, out result);

        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        if (binder == null)
        {
            throw new ArgumentNullException(nameof(binder));
        }

        var propertyInfo = GetType().GetProperty(binder.Name);
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(this, value);
            return true;
        }

        _dynamicData[binder.Name] = value;
        return true;
    }
}