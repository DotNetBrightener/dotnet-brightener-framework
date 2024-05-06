using DotNetBrightener.SimpleUploadService.Models;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.Services;

public static class UploadServiceProviderExtensions
{
    public static async Task ProcessThumbnails(this IUploadServiceProvider uploadServiceProvider,
                                               Stream                      fileUploadStream,
                                               UploadRequestModel          uploadRequestModel,
                                               ILogger                     logger)
    {
        if (!uploadRequestModel.ThumbnailGenerateRequests.Any())
        {
            return;
        }

        foreach (var thumbRequest in uploadRequestModel.ThumbnailGenerateRequests)
        {
            logger.LogInformation("Generating thumbnail with size w:{width} x h:{height}",
                                  thumbRequest.ThumbnailWidth,
                                  thumbRequest.ThumbnailHeight);

            fileUploadStream.Position = 0; // always reset the stream before processing;

            thumbRequest.GeneratedThumbnailStream =
                uploadServiceProvider.ImageResizer.ResizeImageFromStream(fileUploadStream,
                                                                         thumbRequest.ThumbnailWidth,
                                                                         thumbRequest.ThumbnailHeight);
        }
    }
}