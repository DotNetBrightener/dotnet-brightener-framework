using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.MassTransit.Services;

internal class MassTransitEventPublisher(
    IServiceScopeFactory serviceScopeFactory,
    IPublishEndpoint     publishEndpoint,
    ILoggerFactory       loggerFactory)
    : DefaultEventPublisher(serviceScopeFactory, loggerFactory)
{
    public override async Task Publish<T>(T                   eventMessage,
                                          bool                runInBackground = false,
                                          EventMessageWrapper originMessage   = null)
    {
        if (eventMessage is DistributedEventMessage)
        {
            try
            {

                await publishEndpoint.Publish(eventMessage);
            }
            catch (Exception)
            {

                throw;
            }
        }
        else
        {
            await base.Publish(eventMessage, runInBackground);
        }
    }

    public override async Task<TResponse> GetResponse<TResponse, TRequest>(TRequest requestMessage)
    {
        await using (var scope = serviceScopeFactory.CreateAsyncScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<TRequest>>();

            var response = await client.GetResponse<TResponse>(requestMessage);

            return response.Message;
        }
    }

    public override async Task<(TResponse, TErrorResponse)>
        GetResponse<TResponse, TErrorResponse, TRequest>(TRequest requestMessage)
    {
        await using (var scope = serviceScopeFactory.CreateAsyncScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<TRequest>>();

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