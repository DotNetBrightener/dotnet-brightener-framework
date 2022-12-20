using System.Threading.Tasks;

namespace DotNetBrightener.Core.Logging.Loki.Impl
{
    internal interface IJsonSerializer
    {
        Task SerializeAsync(object instance, JsonTextWriter jsonTextWriter);
    }
}
