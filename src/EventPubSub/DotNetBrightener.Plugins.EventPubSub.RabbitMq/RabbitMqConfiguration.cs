namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq;

/// <summary>
///     Configuration options for RabbitMQ EventPubSub provider
/// </summary>
public class RabbitMqConfiguration
{
    /// <summary>
    ///     The hostname of the RabbitMQ server
    /// </summary>
    public string HostName { get; set; }

    /// <summary>
    ///     The port of the RabbitMQ server. Default is 5672.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    ///     The virtual host to connect to. Default is "/".
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    ///     Username for authentication. Default is "guest".
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    ///     Password for authentication. Default is "guest".
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    ///     The name of the subscription (queue consumer identifier).
    ///     Each subscriber should have a unique subscription name to receive its own copy of messages.
    /// </summary>
    public string SubscriptionName { get; set; }

    /// <summary>
    ///     Whether to include the full namespace in exchange names.
    ///     When true, exchange name = Type.FullName. When false, exchange name = Type.Name.
    ///     Default is true.
    /// </summary>
    public bool IncludeNamespaceForExchangeName { get; set; } = true;

    /// <summary>
    ///     Timeout for request-response operations. Default is 2 minutes.
    /// </summary>
    public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    ///     Whether exchanges should be automatically deleted when no longer used. Default is false.
    /// </summary>
    public bool AutoDeleteExchanges { get; set; }

    /// <summary>
    ///     Whether exchanges should survive broker restart. Default is true.
    /// </summary>
    public bool DurableExchanges { get; set; } = true;

    /// <summary>
    ///     Whether queues should survive broker restart. Default is true.
    /// </summary>
    public bool DurableQueues { get; set; } = true;
}
