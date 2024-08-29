using DotNetBrightener.Plugins.EventPubSub;

namespace EventPubSub.WebApiDemo.Contracts;

public class TestMessage : IEventMessage
{
    public string Name { get; set; }
}

public class SomeUpdateMessage : DistributedEventMessage
{
    public string Name { get; set; }
}

public class DistributedTestMessage : RequestMessage
{
    public string Name { get; set; }
}

public class DistributedTestMessageResponse : ResponseMessage<DistributedTestMessage>
{
    public string Name { get; set; }
}