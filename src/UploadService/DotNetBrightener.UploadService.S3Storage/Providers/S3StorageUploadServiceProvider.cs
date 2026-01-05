using System.Reflection;
using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.SimpleUploadService;
using DotNetBrightener.SimpleUploadService.IO;
using DotNetBrightener.SimpleUploadService.Models;
using DotNetBrightener.SimpleUploadService.Services;
using DotNetBrightener.UploadService.S3Storage.Internal;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.UploadService.S3Storage.Providers;

public class S3StorageUploadServiceProvider(
    IMediaFileProvider                   mediaFileStore,
    IContentTypeProvider                 contentTypeProvider,
    ILoggerFactory                       loggerFactory,
    IImageResizer                        imageResizer,
    IHostEnvironment                     webHostEnvironment,
    IOptions<S3StorageConfiguration>     configuration,
    IScheduler                           scheduler,
    Lazy<IS3StorageUploadBackgroundTask> s3StorageUploadBackgroundTask)
    : BaseFileUploadServiceProvider(mediaFileStore, contentTypeProvider, loggerFactory, imageResizer)
{
    private readonly S3StorageConfiguration _configuration = configuration.Value;

    public static readonly MethodInfo UploadAction = typeof(IS3StorageUploadBackgroundTask)
       .GetMethodWithName(nameof(IS3StorageUploadBackgroundTask.UploadLocalFile));

    public override int Priority => 200;

    public override async Task<FileObjectModel> ProcessUpload(Stream             fileUploadStream,
                                                              UploadRequestModel uploadRequestModel,
                                                              string             fileName,
                                                              string             baseUrl)
    {
        var uploadFileName = fileName;

        if (_configuration.UseGuidForFileName &&
            !uploadRequestModel.IsThumbnailUpload)
        {
            var fileExtension = Path.GetExtension(fileName);

            uploadFileName = $"{Guid.CreateVersion7()}{fileExtension}";
        }

        return await base.ProcessUpload(fileUploadStream, uploadRequestModel, uploadFileName, baseUrl);
    }

    protected override async Task<FileObjectModel> UploadFile(Stream             fileUploadStream,
                                                              UploadRequestModel uploadRequestModel,
                                                              string             uploadName,
                                                              string             baseUrl)
    {
        var    folderPath     = uploadRequestModel.Path.ToLower();
        string uploadFileName = uploadName;
        string timeStamp      = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        // only process if uploading file is not a thumbnail
        if (!uploadRequestModel.IsThumbnailUpload)
        {
            uploadName = SanitizeFileName(uploadName);

            var fileNameExtensions = Path.GetExtension(uploadName);

            uploadFileName = !string.IsNullOrEmpty(fileNameExtensions)
                                 ? uploadName.Replace(fileNameExtensions,
                                                      $"_{timeStamp}{fileNameExtensions}")
                                 : uploadName + $"_{timeStamp}";
        }

        var contentType = uploadRequestModel.ContentType ?? "application/octet-stream";

        if (uploadRequestModel.UploadInBackground ||
            _configuration.UploadInBackground)
        {
            Logger.LogInformation("Scheduling upload file {fileName} in background", uploadFileName);

            var tempDownloadFolder = EnsureTempDownloadFolderExists();

            await ScheduleUploadTask(fileUploadStream,
                                     tempDownloadFolder,
                                     uploadFileName,
                                     folderPath,
                                     contentType);
        }
        else
        {
            await s3StorageUploadBackgroundTask.Value
                                           .UploadFromStream(fileUploadStream,
                                                             folderPath,
                                                             uploadFileName,
                                                             contentType);
        }

        var relativeUrl = $"{_configuration.RetrieveFileEndpoint}/{folderPath}/{uploadFileName}";

        var fileObjectModel = new FileObjectModel
        {
            Folder      = folderPath,
            Size        = fileUploadStream.Length,
            RelativeUrl = relativeUrl,
            Name        = uploadFileName,
            Mime        = contentType
        };

        if (!string.IsNullOrEmpty(baseUrl))
        {
            fileObjectModel.AbsoluteUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();
        }

        return fileObjectModel;
    }

    protected override async Task<FileObjectModel> ProcessUploadThumbnail(Stream             thumbnailStream,
                                                                          UploadRequestModel originalUploadRequest,
                                                                          string             originalFileName,
                                                                          string             baseUrl,
                                                                          int                thumbWidth  = 0,
                                                                          int                thumbHeight = 0)
    {
        var fileNameToUpload = ThumbnailNameUtils.GetThumbnailFileName(originalFileName,
                                                                       thumbWidth,
                                                                       thumbHeight);

        return await UploadFile(thumbnailStream,
                                new UploadRequestModel
                                {
                                    Path               = originalUploadRequest.Path,
                                    ContentType        = originalUploadRequest.ContentType,
                                    UploadInBackground = true,
                                    IsThumbnailUpload  = true
                                },
                                fileNameToUpload,
                                baseUrl);
    }

    private string EnsureTempDownloadFolderExists()
    {
        var tempDownloadFolder = Path.Combine(webHostEnvironment.ContentRootPath,
                                              _configuration.TempDownloadFolder);

        if (!Directory.Exists(tempDownloadFolder))
        {
            Directory.CreateDirectory(tempDownloadFolder);
        }

        return tempDownloadFolder;
    }

    private async Task ScheduleUploadTask(Stream fileUploadStream,
                                          string tempDownloadFolder,
                                          string uploadFileName,
                                          string folderPath,
                                          string contentType)
    {
        if (fileUploadStream.Length == 0)
        {
            Logger.LogError("Cannot perform upload. Stream is empty.");

            throw new InvalidOperationException("Cannot perform upload. Stream is empty.");
        }

        var tmpLocalFilePath = Path.Combine(tempDownloadFolder, uploadFileName);

        await using (FileStream fileStream = File.Create(tmpLocalFilePath))
        {
            if (fileUploadStream.Position > 0)
            {
                fileUploadStream.Position = 0;
            }

            await fileUploadStream.CopyToAsync(fileStream);

            Logger.LogInformation("Created temp file {tempFilePath} for uploading. File size: {fileSize:N}. Stream size: {streamSize:N}",
                                  tmpLocalFilePath,
                                  fileStream.Length,
                                  fileUploadStream.Length);
        }

        // Schedule the task to upload file to blob in background to prevent the client from waiting
        scheduler.ScheduleTaskOnce(UploadAction,
                                   tmpLocalFilePath,
                                   folderPath,
                                   uploadFileName,
                                   contentType);

        Logger.LogInformation("File {fileName} has been scheduled to upload in background", uploadFileName);
    }
}