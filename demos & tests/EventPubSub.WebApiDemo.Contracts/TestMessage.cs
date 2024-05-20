using DotNetBrightener.Plugins.EventPubSub;

namespace EventPubSub.WebApiDemo.Contracts;

public class TestMessage : IEventMessage
{
    public string Name { get; set; }
}

public class DistributedTestMessage : IDistributedEventMessage
{
    public string Name { get; set; }
}