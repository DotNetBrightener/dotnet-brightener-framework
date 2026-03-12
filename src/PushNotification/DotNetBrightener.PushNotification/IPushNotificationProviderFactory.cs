namespace DotNetBrightener.PushNotification;

public interface IPushNotificationProviderFactory
{
    IPushNotificationProvider GetProvider(string providerType);
    IEnumerable<IPushNotificationProvider> GetAllProviders();
    IPushNotificationProvider GetProviderForPlatform(string devicePlatform);
}
