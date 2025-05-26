namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal class DefaultServiceBusMessageProcessor<TEventMessage>(IServiceProvider serviceProvider)
    : ServiceBusMessageProcessor<TEventMessage>(serviceProvider)
    where TEventMessage : EventMessageWrapper;