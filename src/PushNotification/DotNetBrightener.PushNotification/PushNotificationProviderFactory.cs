namespace DotNetBrightener.PushNotification;

public class PushNotificationProviderFactory : IPushNotificationProviderFactory
{
    private readonly IEnumerable<IPushNotificationProvider> _providers;

    public PushNotificationProviderFactory(IEnumerable<IPushNotificationProvider> providers)
    {
        _providers = providers;
    }

    public IPushNotificationProvider GetProvider(string providerType)
    {
        return _providers.FirstOrDefault(p => p.PushNotificationType.Equals(providerType, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<IPushNotificationProvider> GetAllProviders()
    {
        return _providers;
    }

    public IPushNotificationProvider GetProviderForPlatform(string devicePlatform)
    {
        return devicePlatform?.ToLowerInvariant() switch
        {
            "ios" => GetProvider(PushNotificationEndpointType.Ios),
            "android" => GetProvider(PushNotificationEndpointType.FirebaseCloudMessaging),
            "web" => GetProvider(PushNotificationEndpointType.FirebaseCloudMessaging),
            _ => null
        };
    }
}
