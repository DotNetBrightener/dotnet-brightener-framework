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
    public static AppClientManagerBuilder WithDbContextConfig(this AppClientManagerBuilder serviceCollection,
                                                              Action<DbContextOptionsBuilder> configure)
    {
        serviceCollection.Services.AddDbContext<AppClientDbContext>(configure);

        serviceCollection.Services.AddSingleton<IMemoryCache, MemoryCache>();
        serviceCollection.Services.AddScoped<IAppClientRepository, AppClientRepository>();

        serviceCollection.Services.Replace(ServiceDescriptor.Scoped<IAppClientManager, AppClientManager>());

        return serviceCollection;
    }
}