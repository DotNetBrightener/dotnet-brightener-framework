using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.Modular.Extensions;

public class DefaultSpaModuleHandler : ISpaModuleHandler
{
    public Task ProcessSpaRequest(HttpResponse response, IFileProvider fileProvider)
    {
        var indexHtmlFile = fileProvider.GetFileInfo("/index.html");
        if (indexHtmlFile.Exists)
        {
            response.StatusCode  = (int) HttpStatusCode.OK;
            response.ContentType = "text/html";
            return response.SendFileAsync(indexHtmlFile);
        }

        return Task.CompletedTask;
    }
}