using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VampireCoder.SharedUtils.DependencyInjection;

public static class InterceptedServiceCollectionExtensions
{
    public static void AddInterceptedScoped<TService, TImplementation, TInterceptor>(
        this IServiceCollection serviceCollection)
        where TService : class
        where TImplementation : class, TService
        where TInterceptor : class, IInterceptor
    {
        serviceCollection.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
        serviceCollection.TryAddTransient<TInterceptor>();

        serviceCollection.AddScoped<TService>(provider =>
        {
            var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();
            var interceptor    = provider.GetRequiredService<TInterceptor>();
            var implementation = ActivatorUtilities.CreateInstance<TImplementation>(provider);

            return proxyGenerator.CreateInterfaceProxyWithTarget<TService>(implementation, interceptor);
        });
    }

    public static void AddInterceptedSingleton<TService, TImplementation, TInterceptor>(
        this IServiceCollection serviceCollection)
        where TService : class
        where TImplementation : class, TService
        where TInterceptor : class, IInterceptor
    {
        serviceCollection.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
        serviceCollection.TryAddTransient<TInterceptor>();

        serviceCollection.AddSingleton<TService>(provider =>
        {
            var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();
            var interceptor    = provider.GetRequiredService<TInterceptor>();
            var implementation = ActivatorUtilities.CreateInstance<TImplementation>(provider);

            return proxyGenerator.CreateInterfaceProxyWithTarget<TService>(implementation, interceptor);
        });
    }

    public static void AddInterceptedTransient<TService, TImplementation, TInterceptor>(
        this IServiceCollection serviceCollection)
        where TService : class
        where TImplementation : class, TService
        where TInterceptor : class, IInterceptor
    {
        serviceCollection.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
        serviceCollection.TryAddTransient<TInterceptor>();

        serviceCollection.AddTransient<TService>(provider =>
        {
            var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();
            var interceptor    = provider.GetRequiredService<TInterceptor>();
            var implementation = ActivatorUtilities.CreateInstance<TImplementation>(provider);

            return proxyGenerator.CreateInterfaceProxyWithTarget<TService>(implementation, interceptor);
        });
    }
}