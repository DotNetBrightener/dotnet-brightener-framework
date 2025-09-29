using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.SimpleUploadService.Services;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.IO;

public interface IMediaFileProvider : ISystemFileProvider;

public class MediaFileProvider : SystemFileProvider, IMediaFileProvider
{
    public MediaFileProvider(IUploadRootPathProvider        uploadRootPathProvider,
                             UploadServiceConfigurationBuilder uploadServiceConfig,
                             ILogger<MediaFileProvider> logger)
        : base(uploadRootPathProvider.UploadRootPath,
               uploadServiceConfig.UploadFolder,
               logger)
    {
    }
}