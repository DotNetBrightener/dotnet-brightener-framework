using DotNetBrightener.SimpleUploadService.Models;

namespace DotNetBrightener.SimpleUploadService.Services;

public static class UploadServiceProviderExtensions
{
    public static async Task ProcessThumbnail(this IUploadServiceProvider uploadServiceProvider,
                                              Stream                      fileUploadStream,
                                              UploadRequestModel          uploadRequestModel)
    {
        foreach (var thumbRequest in uploadRequestModel.ThumbnailGenerateRequests)
        {
            fileUploadStream.Position = 0; // always reset the stream before processing;

            thumbRequest.GeneratedThumbnailStream =
                uploadServiceProvider.ImageResizer.ResizeImageFromStream(fileUploadStream,
                                                                         thumbRequest.ThumbnailWidth,
                                                                         thumbRequest.ThumbnailHeight);
        }
    }
}