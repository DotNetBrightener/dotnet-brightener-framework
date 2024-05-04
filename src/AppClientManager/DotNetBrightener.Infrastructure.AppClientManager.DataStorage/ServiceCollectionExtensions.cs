// ReSharper disable CheckNamespace

using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    [Obsolete($"Use {nameof(WithStorage)} instead.")]
    public static AppClientManagerBuilder WithDbContextConfig(this AppClientManagerBuilder    serviceCollection,
                                                              Action<DbContextOptionsBuilder> configure)
    {
        serviceCollection.Services.AddDbContext<AppClientDbContext>(configure);

        serviceCollection.Services.AddSingleton<IMemoryCache, MemoryCache>();
        serviceCollection.Services.AddScoped<IAppClientRepository, AppClientRepository>();

        serviceCollection.Services.Replace(ServiceDescriptor.Scoped<IAppClientManager, AppClientManager>());

        return serviceCollection;
    }

    public static AppClientManagerBuilder WithStorage(this AppClientManagerBuilder appClientManagerBuilder)
    {
        appClientManagerBuilder.Services.AddSingleton<IMemoryCache, MemoryCache>();
        appClientManagerBuilder.Services.AddScoped<IAppClientRepository, AppClientRepository>();
        appClientManagerBuilder.Services.Replace(ServiceDescriptor.Scoped<IAppClientManager, AppClientManager>());

        return appClientManagerBuilder;
    }
}