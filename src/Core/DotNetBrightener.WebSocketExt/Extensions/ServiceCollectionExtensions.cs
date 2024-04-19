using System.Reflection;
using DotNetBrightener.WebSocketExt;
using DotNetBrightener.WebSocketExt.Internal;
using DotNetBrightener.WebSocketExt.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add the websocket auth token generator service. This should be called only once for the application
    /// </summary>
    /// <typeparam name="TTokenGenerator"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddWebSocketAuthTokenGenerator<TTokenGenerator>(this IServiceCollection services)
        where TTokenGenerator : class, IWebSocketAuthTokenGenerator
    {
        var serviceDescriptor = ServiceDescriptor.Scoped<IWebSocketAuthTokenGenerator, TTokenGenerator>();

        if (services.Any(x => x.ServiceType == typeof(IWebSocketAuthTokenGenerator)))
        {
            services.Replace(serviceDescriptor);
        }
        else
        {
            services.Add(serviceDescriptor);
        }

        return services;
    }

    public static IServiceCollection AddWebSocketCommandServices(this IServiceCollection services,
                                                                 IConfiguration          configuration,
                                                                 params Assembly[]       assemblies)
    {
        var types = assemblies.FilterSkippedAssemblies()
                              .GetDerivedTypes<IWebsocketCommandHandler>()
                              .ToList();

        if (types.Count == 0)
        {
            throw new InvalidOperationException("No websocket command handling service found");
        }

        var commandMetadata = new WebSocketCommandMetadata();

        foreach (var type in types)
        {
            if (type.GetCustomAttribute<WebSocketCommandAttribute>() is not { } attribute)
            {
                throw new
                    InvalidOperationException($"The websocket command handling service {type.FullName} must have [WebSocketCommand] attribute define with its action");
            }

            commandMetadata.Add(attribute.CommandName, type);
            services.AddScoped(typeof(IWebsocketCommandHandler), type);
            services.AddScoped(type);
        }

        services.AddSingleton(commandMetadata);
        services.AddSingleton<IConnectionManager, DefaultConnectionManager>();
        services.AddScoped<IWebSocketUserAuthExchanger, DefaultWebSocketUserAuthExchanger>();
        services.Configure<WebSocketExtOptions>(configuration.GetSection("WebSocketEtx"));

        return services;
    }
}