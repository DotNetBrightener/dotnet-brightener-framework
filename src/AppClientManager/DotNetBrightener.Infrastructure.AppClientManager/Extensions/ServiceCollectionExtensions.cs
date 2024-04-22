using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.Options;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static AppClientManagerBuilder AddAppClientManager(this IServiceCollection serviceCollection,
                                                              IConfiguration          configuration,
                                                              Action<CorsOptions>     setupAction = null)
    {
        serviceCollection.AddCors();

        if (setupAction is not null)
            serviceCollection.Configure(setupAction);


        serviceCollection.Configure<AppClientConfig>(configuration.GetSection(nameof(AppClientConfig)));

        var appClientManagerBuilder = new AppClientManagerBuilder
        {
            Services = serviceCollection
        };
        
        serviceCollection.AddSingleton(appClientManagerBuilder);

        serviceCollection.AddScoped<IAppClientManager, InMemoryAppClientManager>();
        serviceCollection.AddScoped<IAppBundleDetectionService, UserAgentBasedAppBundleDetectionService>();
        serviceCollection.AddScoped<IAppBundleDetectionService, HttpHeaderBaseAppBundleDetectionService>();

        return appClientManagerBuilder;
    }
}