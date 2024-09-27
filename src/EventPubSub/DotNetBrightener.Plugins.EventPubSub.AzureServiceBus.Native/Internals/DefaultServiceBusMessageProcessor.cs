namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Internals;

internal class DefaultServiceBusMessageProcessor<TEventMessage>(IServiceProvider serviceProvider)
    : ServiceBusMessageProcessor<TEventMessage>(serviceProvider)
    where TEventMessage : EventMessageWrapper;