using DotNetBrightener.Plugins.EventPubSub.Distributed.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace

namespace DotNetBrightener.Plugins.EventPubSub;

public static class EventPublisherExtensions
{
    /// <summary>
    ///     Sends a request message and wait for the response message.
    /// </summary>
    /// <typeparam name="TResponse">
    ///     The type of the response message
    /// </typeparam>
    /// <typeparam name="TRequest">
    ///     The type of the request message
    /// </typeparam>
    /// <param name="eventPublisher"></param>
    /// <param name="requestMessage">
    ///     The request message
    /// </param>
    /// <returns>
    ///     The message responses to the <paramref name="requestMessage"/>
    /// </returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task<TResponse> GetResponse<TResponse, TRequest>(this IEventPublisher eventPublisher,
                                                                         TRequest requestMessage)

        where TRequest : RequestMessage
        where TResponse : class, IResponseMessage<TRequest>, new()
    {
        if (eventPublisher is not DistributedEventPublisher massTransitEventPublisher)
        {
            throw new NotImplementedException();
        }

        await using (var scope = massTransitEventPublisher.ServiceScopeFactory.CreateAsyncScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<TRequest>>();

            requestMessage.MachineName = Environment.MachineName;
            requestMessage.CurrentApp  = massTransitEventPublisher.Configurator.AppName;

            if (string.IsNullOrWhiteSpace(requestMessage.OriginApp))
            {
                requestMessage.OriginApp = massTransitEventPublisher.Configurator.AppName;
            }

            var response = await client.GetResponse<TResponse>(requestMessage);

            return response.Message;
        }
    }

    public static async Task<(TResponse, TErrorResponse)> GetResponse<TResponse, TErrorResponse, TRequest>(
        this IEventPublisher eventPublisher,
        TRequest requestMessage)
        where TResponse : class, IResponseMessage<TRequest>, new()
        where TErrorResponse : class, IResponseMessage<TRequest>, new()
        where TRequest : RequestMessage
    {
        if (eventPublisher is not DistributedEventPublisher massTransitEventPublisher)
        {
            throw new NotImplementedException();
        }

        await using (var scope = massTransitEventPublisher.ServiceScopeFactory.CreateAsyncScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<TRequest>>();

            requestMessage.MachineName = Environment.MachineName;
            requestMessage.CurrentApp  = massTransitEventPublisher.Configurator.AppName;

            if (string.IsNullOrWhiteSpace(requestMessage.OriginApp))
            {
                requestMessage.OriginApp = massTransitEventPublisher.Configurator.AppName;
            }

            var response = await client.GetResponse<TResponse, TErrorResponse>(requestMessage);

            if (response.Is(out Response<TResponse> successResponse))
            {
                return (successResponse.Message, null);
            }

            if (response.Is(out Response<TErrorResponse> errorResponse))
            {
                return (null, errorResponse.Message);
            }

            throw new InvalidOperationException("Unable to obtain response with expected type");
        }
    }
}