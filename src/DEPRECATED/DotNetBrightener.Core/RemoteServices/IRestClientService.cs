using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.RemoteServices;

/// <summary>
///     Provides the functionalities for making Http Requests
/// </summary>
public interface IRestClientService
{
    /// <summary>
    ///     Performs the HTTP request with given method and urls
    /// </summary>
    /// <typeparam name="TResponse">The expected type of the response object</typeparam>
    /// <param name="method"></param>
    /// <param name="requestUrl"></param>
    /// <param name="body"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    Task<TResponse> Request<TResponse>(HttpMethod          method,
                                       string              requestUrl,
                                       object              body    = null,
                                       NameValueCollection headers = null);

    Task<TResponse> Get<TResponse>(string requestUrl, NameValueCollection headers = null);

    Task<TResponse> Post<TResponse>(string requestUrl, object body = null, NameValueCollection headers = null);

    Task<TResponse> Put<TResponse>(string requestUrl, object body = null, NameValueCollection headers = null);

    Task<TResponse> Delete<TResponse>(string requestUrl, NameValueCollection headers = null);
}