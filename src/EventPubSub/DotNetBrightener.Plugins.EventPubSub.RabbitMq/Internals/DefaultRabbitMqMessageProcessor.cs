namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal class DefaultRabbitMqMessageProcessor<TEventMessage>(IServiceProvider serviceProvider)
    : RabbitMqMessageProcessor<TEventMessage>(serviceProvider)
    where TEventMessage : EventMessageWrapper, new();
