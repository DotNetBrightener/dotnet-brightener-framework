using DotNetBrightener.Extensions.ProblemsResult.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AspNet.Extensions.SelfDocumentedProblemResult.UI.Services;

internal class UIEndpointsResourceMapper(IUIResourcesReader reader)
{
    private readonly IUIResourcesReader _reader = Guard.ThrowIfNull(reader);

    public IEnumerable<IEndpointConventionBuilder> Map(IEndpointRouteBuilder   builder,
                                                       ErrorsDocTrackerOptions errorsDocTrackerOptions)
    {
        var endpoints = new List<IEndpointConventionBuilder>();

        var resources = _reader.UIResources;

        endpoints.Add(builder.MapGet($"{errorsDocTrackerOptions.UiPath}",
                                     async context =>
                                     {
                                         await ProcessMainUIPage(errorsDocTrackerOptions, resources, context);
                                     }));

        endpoints.Add(builder.MapGet($"{errorsDocTrackerOptions.UiPath}/README.md",
                                     async context =>
                                     {
                                         await ProcessDefaultReadmeForDocsify(errorsDocTrackerOptions,
                                                                              resources,
                                                                              context);
                                     }));

        return endpoints;
    }

    private static async Task ProcessDefaultReadmeForDocsify(ErrorsDocTrackerOptions errorsDocTrackerOptions,
                                                             IEnumerable<UIResource> resources,
                                                             HttpContext             context)
    {
        var resource =
            resources.FirstOrDefault(r => r.FileName.Equals("README.md",
                                                            StringComparison
                                                               .OrdinalIgnoreCase));

        if (resource is not null)
        {
            resource.Content =
                resource.Content.Replace("#APPLICATION_NAME#",
                                         errorsDocTrackerOptions.ApplicationName);
            resource.Content =
                resource.Content.Replace("#COPYRIGHT_YEAR#",
                                         DateTime.Today.Year.ToString());

            context.Response.ContentType = resource.ContentType;
            await context.Response.WriteAsync(resource.Content).ConfigureAwait(false);
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.CompleteAsync();
        }
    }

    private static async Task ProcessMainUIPage(ErrorsDocTrackerOptions errorsDocTrackerOptions,
                                                IEnumerable<UIResource> resources,
                                                HttpContext             context)
    {
        var resource =
            resources.First(r => r.ContentType == ContentType.HTML &&
                                 r.FileName == "index.html");

        resource.Content =
            resource.Content.Replace("#APPLICATION_NAME#",
                                     errorsDocTrackerOptions.ApplicationName);
        resource.Content =
            resource.Content.Replace("#COPYRIGHT_YEAR#", DateTime.Today.Year.ToString());

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
            }

            return Task.CompletedTask;
        });

        context.Response.ContentType = resource.ContentType;
        await context.Response.WriteAsync(resource.Content).ConfigureAwait(false);
    }
}