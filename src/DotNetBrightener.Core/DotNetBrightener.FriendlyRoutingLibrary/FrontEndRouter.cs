using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace DotNetBrightener.FriendlyRoutingLibrary;

/// <summary>
///		Represents a router which handles the routing entries from the <see cref="IFrontEndRoutingEntries"/>
///		and converts to the area/controller/action scheme for ASP.Net routing system
/// </summary>
public class FrontEndRouter : IRouter
{
    private readonly HashSet<string> Keys = new HashSet<string>(new[]
    {
        "area",
        "controller",
        "action",
        "targetType"
    }, StringComparer.OrdinalIgnoreCase);

    private readonly IFrontEndRoutingEntries _entries;
    private readonly IRouter                 _target;
    private readonly string                  _areaName;
    private readonly string                  _controllerName;
    private readonly string                  _actionName;
    private readonly string                  _identifierName;
    private readonly string                  _targetTypeName;
    private readonly bool                    _targetTypeRequired;

    public FrontEndRouter(IFrontEndRoutingEntries entries,
                          IRouter                 target,
                          string                  areaName,
                          string                  controllerName,
                          string                  actionName     = "Index",
                          string                  identifierName = "identifier",
                          string                  targetTypeName = "")
    {
        _target             = target;
        _areaName           = areaName;
        _controllerName     = controllerName;
        _entries            = entries;
        _actionName         = actionName;
        _identifierName     = identifierName;
        _targetTypeRequired = !string.IsNullOrEmpty(targetTypeName);

        Keys.Add(identifierName);

        if (_targetTypeRequired)
        {
            _targetTypeName = targetTypeName;
            Keys.Add(targetTypeName);
        }
    }

    public VirtualPathData GetVirtualPath(VirtualPathContext context)
    {
        string identifier = context.Values["identifier"]?.ToString();

        if (string.IsNullOrEmpty(identifier))
            return null;

        string targetType = context.Values["targetType"]?.ToString();

        if (string.IsNullOrEmpty(targetType))
            return null;

        var displayRouteData = GetContentItemDisplayRoutes(identifier, targetType);

        if (string.Equals(context.Values["area"]?.ToString(), displayRouteData?["area"]?.ToString(),
                          StringComparison.OrdinalIgnoreCase) &&
            string.Equals(context.Values["controller"]?.ToString(), displayRouteData?["controller"]?.ToString(),
                          StringComparison.OrdinalIgnoreCase) &&
            string.Equals(context.Values["action"]?.ToString(), displayRouteData?["action"]?.ToString(),
                          StringComparison.OrdinalIgnoreCase))
        {
            if (_entries.TryGetPath(identifier, targetType, out var path))
            {
                if (context.Values.Count > 4)
                {
                    foreach (var data in context.Values)
                    {
                        if (!Keys.Contains(data.Key))
                        {
                            path = QueryHelpers.AddQueryString(path, data.Key, data.Value.ToString());
                        }
                    }
                }

                return new VirtualPathData(_target, path);
            }
        }


        return null;
    }

    public async Task RouteAsync(RouteContext context)
    {
        var requestPath = context.HttpContext.Request.Path.Value;

        if (_entries.TryGetRoutingEntry(requestPath, out var routingEntry))
        {
            await EnsureRouteData(context, routingEntry);
            await _target.RouteAsync(context);
        }
    }

    private RouteValueDictionary GetContentItemDisplayRoutes(FrontEndRoutingEntry routeEntry)
    {
        if (string.IsNullOrEmpty(routeEntry.ItemId))
        {
            return null;
        }

        RouteValueDictionary routeValueDictionaries = new RouteValueDictionary
        {
            {"area", _areaName},
            {"controller", _controllerName},
            {"action", _actionName},
            {_identifierName, routeEntry.ItemId}
        };

        if (_targetTypeRequired)
        {
            routeValueDictionaries.Add(_targetTypeName, routeEntry.TargetType.FullName);
        }

        return routeValueDictionaries;
    }

    private RouteValueDictionary GetContentItemDisplayRoutes(string itemId, string targetType)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return null;
        }

        RouteValueDictionary routeValueDictionaries = new RouteValueDictionary
        {
            {"area", _areaName},
            {"controller", _controllerName},
            {"action", _actionName},
            {_identifierName, itemId}
        };

        if (_targetTypeRequired)
        {
            routeValueDictionaries.Add(_targetTypeName, targetType);
        }

        return routeValueDictionaries;
    }

    private async Task EnsureRouteData(RouteContext context, FrontEndRoutingEntry routeEntry)
    {
        var displayRoutes = GetContentItemDisplayRoutes(routeEntry);
        if (displayRoutes == null)
        {
            return;
        }

        foreach (var key in Keys)
        {
            if (displayRoutes.ContainsKey(key))
                context.RouteData.Values[key] = displayRoutes[key];
        }
    }
}