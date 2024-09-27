namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Internals;

internal static class TypeToAzureTopicNameUtil
{
    internal static ServiceBusConfiguration ServiceBusConfiguration { get; set; }

    internal static string GetTopicName(this Type type)
    {
        if (string.IsNullOrEmpty(type.Namespace))
            throw new InvalidOperationException("EventMessage type must have a valid namespace");

        if (!type.IsAssignableTo(typeof(DistributedEventMessage)))
            throw new
                InvalidOperationException($"EventMessage type must implement {nameof(DistributedEventMessage)} interface");

        if (ServiceBusConfiguration is not null && !ServiceBusConfiguration.IncludeNamespaceForTopicName)
            return type.Name;

        return type.FullName;
    }
}