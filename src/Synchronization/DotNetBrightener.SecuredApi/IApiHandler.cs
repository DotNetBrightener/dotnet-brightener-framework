namespace DotNetBrightener.SecuredApi;

internal interface IApiHandler
{
    Task<SecuredApiResult> ProcessMessage(ApiMessage message);
}