namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

public class ServiceBusConfiguration
{
    public string ConnectionString { get; set; }

    public string SubscriptionName { get; set; }

    public TimeSpan AutoDeleteOnIdle                    { get; set; } = TimeSpan.FromDays(90);
    public TimeSpan DefaultMessageTimeToLive            { get; set; } = TimeSpan.FromDays(5);
    public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; } = TimeSpan.FromMinutes(1);
    public long     MaxSizeInMegabytes                  { get; set; } = 5120;
}