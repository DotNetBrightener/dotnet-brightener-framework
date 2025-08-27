using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.PushNotification.DependencyInjection;

public class PushNotificationConfigurationBuilder(IServiceCollection serviceCollection)
{
    public IServiceCollection ServiceCollection { get; } = serviceCollection;

    public PushNotificationConfigurationBuilder AddProvider<TProvider>()
        where TProvider : class, IPushNotificationProvider
    {
        ServiceCollection.AddScoped<IPushNotificationProvider, TProvider>();
        ServiceCollection.AddScoped<TProvider>();
        return this;
    }

    public PushNotificationConfigurationBuilder AddProvider<TProvider>(TProvider provider)
        where TProvider : class, IPushNotificationProvider
    {
        ServiceCollection.AddSingleton<IPushNotificationProvider>(provider);
        ServiceCollection.AddSingleton(provider);
        return this;
    }

    public PushNotificationConfigurationBuilder AddProvider<TProvider>(Func<IServiceProvider, TProvider> factory)
        where TProvider : class, IPushNotificationProvider
    {
        ServiceCollection.AddScoped<IPushNotificationProvider>(factory);
        ServiceCollection.AddScoped(factory);
        return this;
    }
}
