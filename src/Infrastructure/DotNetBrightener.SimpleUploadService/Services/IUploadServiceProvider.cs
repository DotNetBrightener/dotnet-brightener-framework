using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.SimpleUploadService.IO;
using DotNetBrightener.SimpleUploadService.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.Services;

public interface IUploadServiceProvider
{
    /// <summary>
    ///     The priority of the provider. The higher number will be higher prioritized.
    /// </summary>
    int Priority { get; }

    IImageResizer ImageResizer { get; }

    Task<FileObjectModel> ProcessUpload(Stream             fileUploadStream,
                                        UploadRequestModel uploadRequestModel,
                                        string             fileName,
                                        string             baseUrl);

    Task<string> GenerateSecuredUrl(string absoluteUrl);
}

public class DefaultUploadServiceProvider : IUploadServiceProvider
{
    private readonly IMediaFileProvider   _mediaFileStore;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly ILogger              _logger;

    public DefaultUploadServiceProvider(IMediaFileProvider                    mediaFileStore,
                                        IContentTypeProvider                  contentTypeProvider,
                                        ILogger<DefaultUploadServiceProvider> logger,
                                        IImageResizer                         imageResizer)
    {
        _mediaFileStore      = mediaFileStore;
        _contentTypeProvider = contentTypeProvider;
        _logger              = logger;
        ImageResizer         = imageResizer;
    }

    public int           Priority     => 0;
    public IImageResizer ImageResizer { get; }

    public virtual async Task<FileObjectModel> ProcessUpload(Stream             fileUploadStream,
                                                             UploadRequestModel uploadRequestModel,
                                                             string             fileName,
                                                             string             baseUrl)
    {
        try
        {
            var fileResult = await ProcessUploadFile(fileUploadStream, uploadRequestModel, fileName, baseUrl);

            _contentTypeProvider.TryGetContentType(fileName, out var contentType);

            if (contentType is null ||
                !contentType.Contains("image") ||
                uploadRequestModel.ThumbnailGenerateRequests is null ||
                !uploadRequestModel.ThumbnailGenerateRequests.Any())
                return fileResult;
            
            await this.ProcessThumbnail(fileUploadStream, uploadRequestModel);

            Parallel.ForEach(uploadRequestModel.ThumbnailGenerateRequests,
                             async (thumbnailRequest) =>
                             {
                                 await ProcessUploadThumbnail(thumbnailRequest.GeneratedThumbnailStream,
                                                              fileName,
                                                              baseUrl,
                                                              thumbnailRequest.ThumbnailWidth,
                                                              thumbnailRequest.ThumbnailHeight);
                             });

            return fileResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(), ex, "An error occurred while uploading a media");

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

    protected virtual FileObjectModel CreateFileResult(IFileInfo mediaFile, string currentRequestUrl)
    {
        _contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        var relativeUrl = _mediaFileStore.MapToPublicUrl(mediaFile.PhysicalPath);
        var absoluteUrl = new Uri(new Uri(currentRequestUrl), relativeUrl).ToString();

        return new FileObjectModel
        {
            Name        = mediaFile.Name,
            Size        = mediaFile.Length,
            Folder      = mediaFile.PhysicalPath,
            AbsoluteUrl = absoluteUrl,
            RelativeUrl = relativeUrl,
            // MediaPath   = mediaFile.,
            Mime = contentType ?? "application/octet-stream"
        };
    }

    protected virtual async Task ProcessUploadThumbnail(Stream fileUploadStream,
                                                        string originalFileName,
                                                        string baseUrl,
                                                        int    thumbWidth  = 0,
                                                        int    thumbHeight = 0)
    {
        string saveFolderPath;

        var fileName = originalFileName;

        if (thumbHeight == 0)
        {
            if (thumbWidth == 0)
            {
                return;
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

        await ProcessUpload(fileUploadStream,
                            new UploadRequestModel
                            {
                                Path = saveFolderPath
                            },
                            fileName,
                            baseUrl);
    }

    private async Task<FileObjectModel> ProcessUploadFile(Stream             fileUploadStream,
                                                          UploadRequestModel uploadRequestModel,
                                                          string             fileName,
                                                          string             baseUrl)
    {
        var mediaFilePath = _mediaFileStore.Combine(uploadRequestModel.Path, fileName);
        var fileExtension = Path.GetExtension(mediaFilePath);
        mediaFilePath = mediaFilePath.Replace(fileExtension, $"_{DateTime.Now.ToFileTime()}{fileExtension}");

        await _mediaFileStore.CreateFileFromStream(mediaFilePath, fileUploadStream);

        var mediaFile = await _mediaFileStore.GetFileInfoAsync(mediaFilePath);

        var fileResult = CreateFileResult(mediaFile, baseUrl);

        return fileResult;
    }
}