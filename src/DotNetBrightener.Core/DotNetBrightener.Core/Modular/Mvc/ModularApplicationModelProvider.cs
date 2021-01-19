using DotNetBrightener.Core.Modular.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.Modular.Mvc
{
    /// <summary>
    /// Provides the application model that will automatically assign the module id to the 'area' route value
    /// </summary>
    public class ModularApplicationModelProvider : IApplicationModelProvider
    {
        public int Order => 1000;

        private readonly IEnumerable<ITypeMetadata> _typeMetadataCollection;
        private readonly string _apiRoutePrefix = "api/";

        public ModularApplicationModelProvider(IEnumerable<ITypeMetadata> typeMetadataCollection)
        {
            _typeMetadataCollection = typeMetadataCollection;
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // loops through the entire application and picks all available controllers
            foreach (var controller in context.Result.Controllers)
            {
                var controllerType = controller.ControllerType.AsType();

                // find the associated module and assign the area route value
                var typeMetadata = _typeMetadataCollection.FindMetadata(controllerType);

                var associatedModuleId = typeMetadata?.AssociatedModule?.Alias ??
                                         typeMetadata?.AssociatedModule?.ModuleId;

                if (associatedModuleId != null && associatedModuleId != ModuleEntry.MainModuleIdentifier)
                {
                    controller.RouteValues.Add(RoutingConstants.AreaKey, associatedModuleId);
                }

                if (typeMetadata != null && !string.IsNullOrEmpty(typeMetadata.SubArea))
                {
                    controller.RouteValues.Add(RoutingConstants.SubAreaKey, typeMetadata.SubArea);
                }

                // detect for the [Route] attribute
                var hasAttributeRouteModels =
                    controller.Selectors.Any(selector => selector.AttributeRouteModel != null);

                // detect the [ApiController] attribute
                var apiControllerAttrib = controllerType.GetCustomAttribute<ApiControllerAttribute>();

                // if the controller is ApiController
                if (apiControllerAttrib != null)
                {
                    var modifiedApiPrefix = _apiRoutePrefix + (associatedModuleId != ModuleEntry.MainModuleIdentifier ? associatedModuleId : "");

                    // if [Route] attribute found
                    if (hasAttributeRouteModels)
                    {
                        var attributeRouteModel = controller.Selectors[0].AttributeRouteModel;

                        var newRouteTemplate = attributeRouteModel.Template;

                        if (newRouteTemplate.StartsWith(_apiRoutePrefix))
                        {
                            newRouteTemplate = modifiedApiPrefix + newRouteTemplate.Substring(_apiRoutePrefix.Length - 1);
                        }
                        else
                        {
                            newRouteTemplate = modifiedApiPrefix + "/" + newRouteTemplate;
                        }

                        attributeRouteModel.Template = newRouteTemplate.Replace("//", "/");
                    }
                    else
                    {
                        controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = (modifiedApiPrefix + "/[controller]").Replace("//", "/")
                        };
                    }
                }
                else
                {
                    // if [Route] attribute found
                    if (hasAttributeRouteModels)
                    {
                        var attributeRouteModel = controller.Selectors[0].AttributeRouteModel;

                        var newRouteTemplate = attributeRouteModel.Template;

                        if (!newRouteTemplate.StartsWith("~"))
                        {
                            // append the module id in front of the routing
                            newRouteTemplate = associatedModuleId + "/" + newRouteTemplate;

                            // append the sub area if available
                            if (controller.RouteValues.ContainsKey(RoutingConstants.SubAreaKey))
                            {
                                newRouteTemplate =
                                    $"{controller.RouteValues[RoutingConstants.SubAreaKey]}/{associatedModuleId}/{newRouteTemplate}";
                            }

                            attributeRouteModel.Template = newRouteTemplate;
                        }
                    }
                    else
                    {
                        var routeTemplate = associatedModuleId + "/[controller]";

                        // append the sub area if available
                        if (controller.RouteValues.ContainsKey(RoutingConstants.SubAreaKey))
                        {
                            routeTemplate = $"{controller.RouteValues[RoutingConstants.SubAreaKey]}/{routeTemplate}";
                        }

                        controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = routeTemplate
                        };
                    }
                }

                // assign the controller route value if needed
                if (!controller.RouteValues.ContainsKey(RoutingConstants.ControllerKey))
                    controller.RouteValues.Add(RoutingConstants.ControllerKey, controller.ControllerName);
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
        }
    }
}