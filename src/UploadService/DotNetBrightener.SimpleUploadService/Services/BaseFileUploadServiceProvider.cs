using DotNetBrightener.SimpleUploadService.IO;
using DotNetBrightener.SimpleUploadService.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.Services;

public abstract class BaseFileUploadServiceProvider : IUploadServiceProvider
{
    protected readonly IMediaFileProvider   MediaFileStore;
    protected readonly IContentTypeProvider ContentTypeProvider;
    protected readonly ILogger              Logger;

    public abstract int Priority { get; }

    public IImageResizer ImageResizer { get; init; }

    protected BaseFileUploadServiceProvider(IMediaFileProvider   mediaFileStore,
                                            IContentTypeProvider contentTypeProvider,
                                            ILoggerFactory       loggerFactory,
                                            IImageResizer        imageResizer)
    {
        MediaFileStore      = mediaFileStore;
        ContentTypeProvider = contentTypeProvider;
        Logger              = loggerFactory.CreateLogger(GetType());
        ImageResizer        = imageResizer;
    }

    protected abstract Task<FileObjectModel> UploadFile(Stream             fileUploadStream,
                                                        UploadRequestModel uploadRequestModel,
                                                        string             uploadName,
                                                        string             baseUrl);

    public virtual async Task<FileObjectModel> ProcessUpload(Stream             fileUploadStream,
                                                             UploadRequestModel uploadRequestModel,
                                                             string             fileName,
                                                             string             baseUrl)
    {
        try
        {
            var isImageUpload       = uploadRequestModel.ContentType?.Contains("image") == true;
            var hasThumbnailRequest = uploadRequestModel.ThumbnailGenerateRequests.Any();
            var onlyProcessThumbnail = isImageUpload &&
                                       uploadRequestModel.OnlyUploadThumbnails &&
                                       hasThumbnailRequest;

            FileObjectModel fileResult = null;

            if (!onlyProcessThumbnail)
            {
                fileResult = await UploadFile(fileUploadStream, uploadRequestModel, fileName, baseUrl);
            }

            await this.ProcessThumbnails(fileUploadStream, uploadRequestModel, Logger);

            var modifiedFileName = fileResult?.Name ?? fileName;

            await uploadRequestModel.ThumbnailGenerateRequests
                                    .Where(model => model.GeneratedThumbnailStream is not null &&
                                                    model.GeneratedThumbnailStream.Length > 0)
                                    .ParallelForEachAsync(async (thumbnailRequest) =>
                                     {
                                         if (thumbnailRequest.GeneratedThumbnailStream.Length == 0)
                                         {
                                             Logger.LogError("Thumbnail stream is empty, skipping thumbnail upload");
                                             return;
                                         }

                                         Logger.LogInformation("Uploading thumbnail at size w:{width} x h:{height}",
                                                               thumbnailRequest.ThumbnailWidth,
                                                               thumbnailRequest.ThumbnailHeight);
                                        var result =
                                             await ProcessUploadThumbnail(thumbnailRequest.GeneratedThumbnailStream,
                                                                          uploadRequestModel,
                                                                          modifiedFileName,
                                                                          baseUrl,
                                                                          thumbnailRequest.ThumbnailWidth,
                                                                          thumbnailRequest.ThumbnailHeight);

                                         fileResult ??= result;
                                     });

            return fileResult;
        }
        catch (Exception ex)
        {
            Logger.LogError(new EventId(), ex, "An error occurred while uploading a media");

            return new FileObjectModel
            {
                Name   = fileName,
                Folder = uploadRequestModel.Path,
                Error  = ex.GetFullExceptionMessage()
            };
        }
    }

    public virtual async Task<string> GenerateSecuredUrl(string absoluteUrl)
    {
        return absoluteUrl;
    }

    protected abstract Task<FileObjectModel> ProcessUploadThumbnail(Stream             thumbnailStream,
                                                                    UploadRequestModel originalUploadRequest,
                                                                    string             originalFileName,
                                                                    string             baseUrl,
                                                                    int                thumbWidth  = 0,
                                                                    int                thumbHeight = 0);

    protected static string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars()
                                  .Concat([
                                       ' '
                                   ])
                                  .ToArray();

        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}