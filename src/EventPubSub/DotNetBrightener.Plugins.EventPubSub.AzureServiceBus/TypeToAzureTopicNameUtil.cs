namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

internal static class TypeToAzureTopicNameUtil
{
    internal static string GetTopicName(this Type type)
    {
        if (string.IsNullOrEmpty(type.Namespace))
            throw new InvalidOperationException("EventMessage type must have a valid namespace");

        if (!type.IsAssignableTo(typeof(IDistributedEventMessage)))
            throw new
                InvalidOperationException($"EventMessage type must implement {nameof(IDistributedEventMessage)} interface");

        return type.FullName;
    }
}