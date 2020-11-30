using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace DotNetBrightener.FriendlyRoutingLibrary
{
    public class FrontEndRouter : IRouter
    {
        private static readonly HashSet<string> Keys = new HashSet<string>(new[]
        {
            "area",
            "controller",
            "action",
            "identifier",
            "targetType"
        }, StringComparer.OrdinalIgnoreCase);

        private readonly IFrontEndRoutingEntries _entries;
        private readonly IRouter                 _target;
        private readonly string                  _areaName;
        private readonly string                  _controllerName;

        public FrontEndRouter(IFrontEndRoutingEntries entries,
                              IRouter                 target,
                              string                  areaName,
                              string                  controllerName)
        {
            _target         = target;
            _areaName       = areaName;
            _controllerName = controllerName;
            _entries        = entries;
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

            if (!long.TryParse(routeEntry.ItemId, out var itemId))
                return null;

            return new RouteValueDictionary
            {
                {"area", _areaName},
                {"controller", _controllerName},
                {"action", "Index"},
                {"targetType", routeEntry.TargetType.FullName},
                {"identifier", itemId}
            };
        }

        private RouteValueDictionary GetContentItemDisplayRoutes(string itemId, string targetType)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            if (!long.TryParse(itemId, out var contentId))
                return null;

            return new RouteValueDictionary
            {
                {"area", _areaName},
                {"controller", _controllerName},
                {"action", "Index"},
                {"targetType", targetType},
                {"identifier", contentId}
            };
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
}