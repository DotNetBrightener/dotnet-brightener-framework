using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Native.Extensions;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace DotNetBrightener.Plugins.EventPubSub;

public static class DistributedEventMessageWrapperExtensions
{
    internal static void WithPayload<T>(this EventMessageWrapper msg, T eventMessage)
        where T : DistributedEventMessage
    {
        var serializedMessage = JsonConvert.SerializeObject(eventMessage, JsonConfig.SerializeOptions);

        msg.Payload =
            JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedMessage,
                                                                      JsonConfig.DeserializeOptions);
    }

    internal static T GetPayload<T>(this EventMessageWrapper msg) where T : DistributedEventMessage
    {
        var potentialNestedMsg = msg.Payload.TryGetValue("message", out var payloadValue);
        var jsonPayload = potentialNestedMsg
                              ? JsonConvert.SerializeObject(payloadValue, JsonConfig.SerializeOptions)
                              : JsonConvert.SerializeObject(msg.Payload, JsonConfig.SerializeOptions);

        T result = JsonConvert.DeserializeObject<T>(jsonPayload, JsonConfig.DeserializeOptions);

        return result;
    }

    public static string ToJson(this EventMessageWrapper msg)
        => JsonConvert.SerializeObject(msg, JsonConfig.SerializeOptions);

    public static EventMessageWrapper DeserializeToMessage<TEventMessage>(this string msg)
        where TEventMessage : EventMessageWrapper
        => JsonConvert.DeserializeObject<TEventMessage>(msg, JsonConfig.DeserializeOptions);
}