namespace DotNetBrightener.SecuredApi;

internal class SecuredApiHandlerRouter : Dictionary<string, SecuredApiHandlerRoutingMetadata>
{
    internal bool MiddlewareRegistered { get; set; }
}

internal struct SecuredApiHandlerRoutingMetadata
{
    public string RoutePattern { get; set; }

    public Type HandlerType { get; set; }

    public HttpMethod HttpMethod { get; set; }
}