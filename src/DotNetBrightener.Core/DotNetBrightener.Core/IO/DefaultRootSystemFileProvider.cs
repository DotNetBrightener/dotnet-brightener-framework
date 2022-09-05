using Microsoft.AspNetCore.Hosting;

namespace DotNetBrightener.Core.IO;

public class DefaultRootSystemFileProvider : SystemFileProvider, IRootSystemFileProvider
{
    public DefaultRootSystemFileProvider(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment.WebRootPath, string.Empty)
    {
    }
}