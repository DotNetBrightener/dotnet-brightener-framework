using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.Modular.Extensions;

public interface ISpaModuleHandler
{
    Task ProcessSpaRequest(HttpResponse response, IFileProvider fileProvider);
}