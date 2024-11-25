namespace DotNetBrightener.Plugins.EventPubSub.MassTransit.RabbitMq;

public class RabbitMqConfiguration
{
    public string Host                         { get; init; }
    public string Username                     { get; init; }
    public string Password                     { get; init; }
    public string VirtualHost                  { get; init; }
    public string SubscriptionName             { get; init; }
    public bool   IncludeNamespaceForTopicName { get; init; } = true;
}