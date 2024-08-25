using DotNetBrightener.Plugins.EventPubSub;

namespace EventPubSub.WebApiDemo.Contracts;

public class TestMessage : IEventMessage
{
    public string Name { get; set; }
}

public class DistributedTestMessage : IRequestMessage
{
    public string Name { get; set; }
}

public class DistributedTestMessageResponse : IResponseMessage<DistributedTestMessage>
{
    public string Name { get; set; }
}