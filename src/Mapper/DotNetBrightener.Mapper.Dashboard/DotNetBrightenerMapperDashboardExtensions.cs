using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Mapper.Dashboard;

/// <summary>
///     Extension methods for configuring the DotNetBrightener Mapper Dashboard in an ASP.NET Core application.
/// </summary>
public static class DotNetBrightenerMapperDashboardExtensions
{
    /// <summary>
    ///     Adds DotNetBrightener Mapper Dashboard services to the service collection.
    /// </summary>
    /// <param name="services">
    ///     The service collection.
    /// </param>
    /// <param name="configure">
    ///     Optional action to configure dashboard options.
    /// </param>
    /// <returns>
    ///     The service collection for chaining.
    /// </returns>
    public static IServiceCollection AddDotNetBrightenerMapperDashboard(
        this IServiceCollection services,
        Action<DotNetBrightenerMapperDashboardOptions>? configure = null)
    {
        var options = new DotNetBrightenerMapperDashboardOptions();
        configure?.Invoke(options);

        services.AddSingleton(Options.Create(options));
        services.AddSingleton<MapperDashboardService>();

        return services;
    }

    /// <summary>
    ///     Maps the DotNetBrightener Mapper Dashboard endpoints to the application.
    /// </summary>
    /// <param name="app">
    ///     The endpoint route builder.
    /// </param>
    /// <returns>
    ///     The endpoint route builder for chaining.
    /// </returns>
    public static IEndpointRouteBuilder MapDotNetBrightenerMapperDashboard(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetService<IOptions<DotNetBrightenerMapperDashboardOptions>>()?.Value
            ?? new DotNetBrightenerMapperDashboardOptions();

        var routePrefix = options.RoutePrefix.TrimEnd('/');

        // Main dashboard HTML page
        var dashboardEndpoint = app.MapGet(routePrefix, async context =>
        {
            var dashboardService = context.RequestServices.GetRequiredService<MapperDashboardService>();
            var html = dashboardService.GetDashboardHtml();
            
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(html);
        });

        // JSON API endpoint
        if (options.EnableJsonApi)
        {
            app.MapGet($"{routePrefix}/api/dnb-mapping-types", async context =>
            {
                var dashboardService = context.RequestServices.GetRequiredService<MapperDashboardService>();
                var targets = dashboardService.GetMappingTypes();
                
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                // Convert to serializable format (remove Type references)
                var result = targets.Select(m => new
                {
                    sourceTypeName = m.SourceTypeName,
                    sourceTypeSimpleName = m.SourceTypeSimpleName,
                    sourceTypeNamespace = m.SourceTypeNamespace,
                    sourceMembers = m.SourceMembers.Select(sm => new
                    {
                        sm.Name,
                        sm.TypeName,
                        sm.IsProperty,
                        sm.IsNullable,
                        sm.IsRequired,
                        sm.IsInitOnly,
                        sm.IsReadOnly,
                        sm.IsCollection,
                        sm.Attributes
                    }),
                    targets = m.MappingTypes.Select(f => new
                    {
                        targetTypeName = f.MappingTypeName,
                        targetTypeSimpleName = f.MappingTypeSimpleName,
                        targetTypeNamespace = f.MappingTypeNamespace,
                        f.TypeKind,
                        f.HasConstructor,
                        f.HasProjection,
                        f.HasToSource,
                        f.NullableProperties,
                        f.CopyAttributes,
                        f.ConfigurationTypeName,
                        excludedProperties = f.ExcludedProperties,
                        includedProperties = f.IncludedProperties,
                        nestedTargets = f.NestedTargetTypes.Select(n => n.FullName),
                        members = f.Members.Select(fm => new
                        {
                            fm.Name,
                            fm.TypeName,
                            fm.IsProperty,
                            fm.IsNullable,
                            fm.IsRequired,
                            fm.IsInitOnly,
                            fm.IsReadOnly,
                            fm.IsNestedTarget,
                            fm.IsCollection,
                            fm.MappedFromProperty,
                            fm.Attributes
                        })
                    })
                });

                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
            });
        }

        // Apply authentication if configured
        if (options.RequireAuthentication && !string.IsNullOrEmpty(options.AuthenticationPolicy))
        {
            dashboardEndpoint.RequireAuthorization(options.AuthenticationPolicy);
        }
        else if (options.RequireAuthentication)
        {
            dashboardEndpoint.RequireAuthorization();
        }

        return app;
    }
}
