using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed;

public static class EventPublisherRequestRespondExtensions
{
    public static async Task<TResponse> GetResponse<TRequest, TResponse>(this IEventPublisher publisher,
                                                                         TRequest             requestMessage)
        where TRequest : RequestMessage
        where TResponse : ResponseMessage<TRequest>
    {
        if (publisher is DistributedEventPublisher azureEsbPublisher)
        {
            using var scope = azureEsbPublisher.ServiceScopeFactory.CreateScope();

            var serviceBusMessagePublisher =
                scope.ServiceProvider.GetRequiredService<IDistributedMessagePublisher>();

            return await serviceBusMessagePublisher.GetResponse<TRequest, TResponse>(requestMessage);
        }

        throw new InvalidOperationException("Not supported operation");
    }
}