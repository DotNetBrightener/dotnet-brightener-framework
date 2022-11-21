using Microsoft.Extensions.Hosting;

namespace DotNetBrightener.SimpleUploadService.Services;

public interface IUploadRootPathProvider
{
    /// <summary>
    ///     Specifies the root path of the folder which stores the uploaded files
    /// </summary>
    string UploadRootPath { get; }
}

public class DefaultUploadRootPathProvider : IUploadRootPathProvider
{
    public string UploadRootPath { get; }

    public DefaultUploadRootPathProvider(IHostEnvironment hostEnvironment)
    {
        this.UploadRootPath = hostEnvironment.ContentRootPath;
    }
}