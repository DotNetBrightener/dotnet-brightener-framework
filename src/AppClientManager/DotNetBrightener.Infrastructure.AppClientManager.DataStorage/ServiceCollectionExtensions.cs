// ReSharper disable CheckNamespace

using DotNetBrightener.Infrastructure.AppClientManager;
using DotNetBrightener.Infrastructure.AppClientManager.DataStorage;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static AppClientManagerBuilder WithStorage(this AppClientManagerBuilder appClientManagerBuilder)
    {
        appClientManagerBuilder.Services.AddSingleton<IMemoryCache, MemoryCache>();
        appClientManagerBuilder.Services.AddScoped<IAppClientRepository, AppClientRepository>();
        appClientManagerBuilder.Services.Replace(ServiceDescriptor.Scoped<IAppClientManager, AppClientManager>());

        return appClientManagerBuilder;
    }
}