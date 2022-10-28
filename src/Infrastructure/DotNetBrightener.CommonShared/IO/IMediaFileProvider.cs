using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.CommonShared.IO;

public interface IMediaFileProvider : ISystemFileProvider, ISingletonDependency
{
}

public class MediaFileProvider : SystemFileProvider, IMediaFileProvider
{
    public MediaFileProvider(IWebHostEnvironment        environment,
                             ILogger<MediaFileProvider> logger)
        : base(environment.ContentRootPath, 
               "Media", 
               logger)
    {
    }
}