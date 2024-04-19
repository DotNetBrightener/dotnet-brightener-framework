using AspNet.Extensions.SelfDocumentedProblemResult;
using AspNet.Extensions.SelfDocumentedProblemResult.UI;
using AspNet.Extensions.SelfDocumentedProblemResult.UI.Services;
using DotNetBrightener.Extensions.ProblemsResult.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapErrorDocsTrackerUI(this IEndpointRouteBuilder       builder,
                                                                   Action<ErrorsDocTrackerOptions>? setupOptions = null)
    {
        var options = new ErrorsDocTrackerOptions();
        setupOptions?.Invoke(options);

        ProblemResultExtensions.UiProblemUrl        = options.UiPath;
        ProblemResultExtensions.HttpContextAccessor = builder.ServiceProvider.GetService<IHttpContextAccessor>();

        var embeddedResourcesAssembly = typeof(UIResource).Assembly;

        var uiEndpointsResourceMapper =
            new UIEndpointsResourceMapper(new UIEmbeddedResourcesReader(embeddedResourcesAssembly));

        var resourcesEndpoints = uiEndpointsResourceMapper.Map(builder, options);


        var problemTypes = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .FilterSkippedAssemblies()
                                    .GetDerivedTypes<IProblemResult>()
                                    .ToList();

        var problemResultEndpoints = new UiProblemResultEndpointsMapper(problemTypes).Map(builder, options);

        return new ErrorDocTrackerUIConventionBuilder([..resourcesEndpoints, ..problemResultEndpoints]);
    }
}