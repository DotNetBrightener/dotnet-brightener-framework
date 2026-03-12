using DotNetBrightener.PushNotification;
using DotNetBrightener.PushNotification.APN;
using DotNetBrightener.PushNotification.DependencyInjection;
using DotNetBrightener.PushNotification.FirebaseIntegration;
using Microsoft.Extensions.Configuration;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static PushNotificationConfigurationBuilder AddPushNotification(
        this IServiceCollection serviceCollection,
        IConfiguration configuration = null)
    {
        serviceCollection.AddScoped<IPushNotificationService, PushNotificationService>();
        serviceCollection.AddScoped<IPushNotificationProviderFactory, PushNotificationProviderFactory>();
        serviceCollection.AddSingleton<IPushNotificationSubscriptionRepository, InMemoryPushNotificationSubscriptionRepository>();

        var builder = new PushNotificationConfigurationBuilder(serviceCollection);
        serviceCollection.AddSingleton(builder);

        return builder;
    }

    public static PushNotificationConfigurationBuilder AddFirebaseCloudMessaging(
        this PushNotificationConfigurationBuilder builder,
        IConfiguration configuration,
        string configurationSectionName = "PushNotification:Firebase")
    {
        builder.ServiceCollection.Configure<PushNotificationFirebaseSettings>(
            configuration.GetSection(configurationSectionName));

        builder.AddProvider<FirebasePushNotificationProvider>();

        return builder;
    }

    public static PushNotificationConfigurationBuilder AddApplePushNotification(
        this PushNotificationConfigurationBuilder builder,
        IConfiguration configuration,
        string configurationSectionName = "PushNotification:Apple")
    {
        builder.ServiceCollection.Configure<ApnSettings>(
            configuration.GetSection(configurationSectionName));

        builder.AddProvider<ApnPushNotificationProvider>();

        return builder;
    }

    public static PushNotificationConfigurationBuilder AddFirebaseCloudMessaging(
        this PushNotificationConfigurationBuilder builder,
        Action<PushNotificationFirebaseSettings> configureOptions)
    {
        builder.ServiceCollection.Configure(configureOptions);
        builder.AddProvider<FirebasePushNotificationProvider>();

        return builder;
    }

    public static PushNotificationConfigurationBuilder AddApplePushNotification(
        this PushNotificationConfigurationBuilder builder,
        Action<ApnSettings> configureOptions)
    {
        builder.ServiceCollection.Configure(configureOptions);
        builder.AddProvider<ApnPushNotificationProvider>();

        return builder;
    }

    public static PushNotificationConfigurationBuilder UseSubscriptionRepository<TRepository>(
        this PushNotificationConfigurationBuilder builder)
        where TRepository : class, IPushNotificationSubscriptionRepository
    {
        builder.ServiceCollection.AddScoped<IPushNotificationSubscriptionRepository, TRepository>();
        return builder;
    }
}
