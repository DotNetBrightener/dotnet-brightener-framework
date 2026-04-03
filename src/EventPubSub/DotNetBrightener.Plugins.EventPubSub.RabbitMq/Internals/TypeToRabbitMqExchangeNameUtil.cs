namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal static class TypeToRabbitMqExchangeNameUtil
{
    internal static RabbitMqConfiguration RabbitMqConfiguration { get; set; }

    internal static string GetExchangeName(this Type type)
    {
        if (string.IsNullOrEmpty(type.Namespace))
            throw new InvalidOperationException("EventMessage type must have a valid namespace");

        if (!type.IsAssignableTo(typeof(IDistributedEventMessage)))
            throw new
                InvalidOperationException($"EventMessage type must implement {nameof(IDistributedEventMessage)} interface");

        if (RabbitMqConfiguration is not null &&
            !RabbitMqConfiguration.IncludeNamespaceForExchangeName)
            return type.Name;

        return type.FullName;
    }
}
