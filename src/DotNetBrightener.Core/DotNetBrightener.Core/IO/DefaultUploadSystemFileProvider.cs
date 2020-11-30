using Microsoft.AspNetCore.Hosting;

namespace DotNetBrightener.Core.IO
{
    /// <summary>
    ///     Provides access to the uploaded files
    /// </summary>
    public interface IUploadSystemFileProvider : ISystemFileProvider
    {
    }

    public class DefaultUploadSystemFileProvider : SystemFileProvider, IUploadSystemFileProvider
    {
        public DefaultUploadSystemFileProvider(IWebHostEnvironment webHostEnvironment)
            : base(webHostEnvironment.WebRootPath, "Uploads")
        {
        }
    }
}