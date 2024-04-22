namespace DotNetBrightener.Core.Logging.Loki;

public interface ILokiHttpClient
{
    Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
}