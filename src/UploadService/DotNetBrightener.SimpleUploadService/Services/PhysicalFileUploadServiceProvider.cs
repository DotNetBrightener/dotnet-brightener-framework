using DotNetBrightener.SimpleUploadService.IO;
using DotNetBrightener.SimpleUploadService.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.Services;

public class PhysicalFileUploadServiceProvider(
    IMediaFileProvider   mediaFileStore,
    IContentTypeProvider contentTypeProvider,
    ILoggerFactory       loggerFactory,
    IImageResizer        imageResizer)
    : BaseFileUploadServiceProvider(mediaFileStore, contentTypeProvider, loggerFactory, imageResizer)
{
    public override int Priority => 0;

    protected override async Task<FileObjectModel> UploadFile(Stream             fileUploadStream,
                                                              UploadRequestModel uploadRequestModel,
                                                              string             uploadName,
                                                              string             baseUrl)
    {
        fileUploadStream.Position = 0;

        uploadName = SanitizeFileName(uploadName);

        var mediaFilePath = MediaFileStore.Combine(uploadRequestModel.Path, uploadName);
        var fileExtension = Path.GetExtension(mediaFilePath);
        mediaFilePath = mediaFilePath.Replace(fileExtension, $"_{DateTime.Now.ToFileTime()}{fileExtension}");

        await MediaFileStore.CreateFileFromStream(mediaFilePath, fileUploadStream);

        var mediaFile = await MediaFileStore.GetFileInfoAsync(mediaFilePath);

        var fileResult = CreateFileResult(mediaFile, baseUrl);
        fileResult.Mime = uploadRequestModel.ContentType ?? "application/octet-stream";

        return fileResult;
    }
    
    /// <summary>
    ///     Defines how to upload the thumbnail from the given stream
    /// </summary>
    /// <param name="thumbnailStream"></param>
    /// <param name="originalUploadRequest"></param>
    /// <param name="originalFileName"></param>
    /// <param name="baseUrl"></param>
    /// <param name="thumbWidth"></param>
    /// <param name="thumbHeight"></param>
    /// <returns></returns>
    protected override async Task<FileObjectModel> ProcessUploadThumbnail(Stream             thumbnailStream,
                                                                         UploadRequestModel originalUploadRequest,
                                                                         string             originalFileName,
                                                                         string             baseUrl,
                                                                         int                thumbWidth  = 0,
                                                                         int                thumbHeight = 0)
    {
        string saveFolderPath;

        var fileName = originalFileName;

        if (thumbHeight == 0)
        {
            if (thumbWidth == 0)
            {
                return null;
            }

            saveFolderPath = Path.Combine("Thumb", $"w_{thumbWidth}");
        }
        else
        {
            var thumbName = thumbWidth == 0
                                ? $"h_{thumbHeight}"
                                : $"w_{thumbWidth}_h_{thumbHeight}";

            saveFolderPath = Path.Combine("Thumb", thumbName);
        }

        return await UploadFile(thumbnailStream,
                                new UploadRequestModel
                                {
                                    Path = saveFolderPath
                                },
                                fileName,
                                baseUrl);
    }

    private FileObjectModel CreateFileResult(IFileInfo mediaFile, string currentRequestUrl)
    {
        var relativeUrl = MediaFileStore.MapToPublicUrl(mediaFile.PhysicalPath);
        var absoluteUrl = new Uri(new Uri(currentRequestUrl), relativeUrl).ToString();

        return new FileObjectModel
        {
            Name        = mediaFile.Name,
            Size        = mediaFile.Length,
            Folder      = mediaFile.PhysicalPath,
            AbsoluteUrl = absoluteUrl,
            RelativeUrl = relativeUrl
        };
    }
}