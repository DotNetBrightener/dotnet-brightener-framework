using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.SecuredApi;

public abstract class BaseApiHandler
{
    public HttpContext HttpContext { get; internal set; }

    public HttpRequest Request { get; internal set; }

    public HttpResponse Response { get; internal set; }
}

public abstract class BaseApiHandler<TRequest, TResponse> : BaseApiHandler, IApiHandler
    where TRequest : class, new()
{
    public virtual async Task<SecuredApiResult> ProcessMessage(ApiMessage message)
    {
        var messageRequest = message.GetEntity<TRequest>();

        var result = await ProcessRequest(messageRequest);

        return SecuredApiResult.FromPayload(result);
    }

    protected abstract Task<TResponse> ProcessRequest(TRequest message);
}

public abstract class BaseApiHandler<TRequest> : BaseApiHandler<TRequest, TRequest>
    where TRequest : class, new();

/// <summary>
///     Represents a request model that does not have any properties.
/// </summary>
public sealed class NullRequestModel;

public abstract class EmptyRequestBaseApiHandler<TResponse> : BaseApiHandler<NullRequestModel, TResponse>
{
    public sealed override async Task<SecuredApiResult> ProcessMessage(ApiMessage message)
    {
        var result = await ProcessRequest();

        return SecuredApiResult.FromPayload(result);
    }

    protected override Task<TResponse> ProcessRequest(NullRequestModel message)
    {
        return null;
    }

    protected abstract Task<TResponse> ProcessRequest();
}