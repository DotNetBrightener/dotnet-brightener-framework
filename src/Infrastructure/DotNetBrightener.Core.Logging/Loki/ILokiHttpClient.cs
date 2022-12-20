using System.Net.Http;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.Logging.Loki
{
    public interface ILokiHttpClient
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
    }
}
