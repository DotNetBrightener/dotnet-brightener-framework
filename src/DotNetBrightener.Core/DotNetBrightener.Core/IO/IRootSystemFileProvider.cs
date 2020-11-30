using Microsoft.AspNetCore.Hosting;

namespace DotNetBrightener.Core.IO
{
    /// <summary>
    ///     Provides the access to files at the root level of the application
    /// </summary>
    public interface IRootSystemFileProvider : ISystemFileProvider
    {
    }

    public class DefaultRootSystemFileProvider : SystemFileProvider, IRootSystemFileProvider
    {
        public DefaultRootSystemFileProvider(IWebHostEnvironment webHostEnvironment)
            : base(webHostEnvironment.WebRootPath, string.Empty)
        {
        }
    }
}