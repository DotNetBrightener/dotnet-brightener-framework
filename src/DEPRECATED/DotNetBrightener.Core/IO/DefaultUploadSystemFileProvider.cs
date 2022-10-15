using Microsoft.AspNetCore.Hosting;

namespace DotNetBrightener.Core.IO;

public class DefaultUploadSystemFileProvider : SystemFileProvider, IUploadSystemFileProvider
{
    public DefaultUploadSystemFileProvider(IWebHostEnvironment webHostEnvironment)
        : base(webHostEnvironment.WebRootPath, "Uploads")
    {
    }
}