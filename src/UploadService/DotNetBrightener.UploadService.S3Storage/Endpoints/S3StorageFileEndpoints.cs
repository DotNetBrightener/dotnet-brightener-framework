using System.Diagnostics;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using DotNetBrightener.SimpleUploadService.Models;
using DotNetBrightener.SimpleUploadService.Services;
using DotNetBrightener.UploadService.S3Storage.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.UploadService.S3Storage.Endpoints;

public static class S3StorageFileEndpoints
{
    /// <summary>
    ///     Registers the endpoint for retrieving files from S3-compatible storage
    /// </summary>
    /// <param name="endpoints">
    ///     The <see cref="IEndpointRouteBuilder"/>
    /// </param>
    public static void MapS3StorageFileEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var configuration = endpoints.ServiceProvider.GetRequiredService<IOptions<S3StorageConfiguration>>();

        var s3FileEndpointGroup = endpoints.MapGroup(configuration.Value.RetrieveFileEndpoint);

        s3FileEndpointGroup.MapGet("{*filePath}",
                                      async (string                            filePath,
                                             IOptions<S3StorageConfiguration>  s3StorageConfiguration,
                                             IHostEnvironment                  environment,
                                             ILoggerFactory                    loggerFactory,
                                             IUploadService                    uploadService,
                                             IContentTypeProvider              contentTypeProvider,
                                             [FromQuery]
                                             int? width = null,
                                             [FromQuery]
                                             int? height = null) =>
                                      {
                                          var logger = loggerFactory.CreateLogger(typeof(S3StorageFileEndpoints));

                                          return await DownloadFileFromS3Storage(filePath,
                                                                                 s3StorageConfiguration.Value,
                                                                                 environment,
                                                                                 uploadService,
                                                                                 contentTypeProvider,
                                                                                 logger,
                                                                                 width,
                                                                                 height);
                                      });
    }

    private static async Task<IResult> DownloadFileFromS3Storage(string                   filePath,
                                                                  S3StorageConfiguration   cfg,
                                                                  IHostEnvironment         environment,
                                                                  IUploadService           uploadService,
                                                                  IContentTypeProvider     contentTypeProvider,
                                                                  ILogger                  logger,
                                                                  int?                     width  = null,
                                                                  int?                     height = null)
    {
        var fileSegments = filePath.Split("/", StringSplitOptions.RemoveEmptyEntries);

        if (fileSegments.Length != 2)
            return Results.BadRequest("Invalid file path format. Expected: {folder}/{filename}");

        var folderPath       = fileSegments[0];
        var fileName         = fileSegments[1];
        var originalFileName = fileName;

        var needResize   = width != null || height != null;
        var needReupload = false;

        if (needResize)
        {
            fileName = ThumbnailNameUtils.GetThumbnailFileName(originalFileName, width ?? 0, height ?? 0);
        }

        // Check local cache first
        var tmpLocalFile = Path.Combine(environment.ContentRootPath, cfg.TempDownloadFolder, fileName);

        if (File.Exists(tmpLocalFile))
        {
            var fileInfo = new FileInfo(tmpLocalFile);

            // Check if cache is still valid
            if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < cfg.CacheExpiration)
            {
                contentTypeProvider.TryGetContentType(tmpLocalFile, out var contentType);
                logger.LogInformation("Response from cached local file {fileName}", fileName);

                return Results.File(tmpLocalFile, contentType ?? "application/octet-stream");
            }
        }

        if (fileName != originalFileName)
        {
            tmpLocalFile = Path.Combine(environment.ContentRootPath, cfg.TempDownloadFolder, originalFileName);

            if (File.Exists(tmpLocalFile))
            {
                var fileInfo = new FileInfo(tmpLocalFile);

                if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < cfg.CacheExpiration)
                {
                    contentTypeProvider.TryGetContentType(tmpLocalFile, out var contentType);

                    logger.LogInformation("Response from cached local file {fileName}", originalFileName);

                    return Results.File(tmpLocalFile, contentType ?? "application/octet-stream");
                }
            }
        }

        logger.LogInformation("No cached file available for {fileName}. Fetching from S3 storage.",
                              originalFileName);

        // Create S3 client
        var credentials = new BasicAWSCredentials(cfg.AccessKey, cfg.SecretKey);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = cfg.ServiceUrl,
            ForcePathStyle = cfg.ForcePathStyle
        };

        using var s3Client = new AmazonS3Client(credentials, s3Config);

        var key = $"{folderPath}/{fileName}";

        try
        {
            var swStart = Stopwatch.GetTimestamp();

            var getRequest = new GetObjectRequest
            {
                BucketName = cfg.BucketName,
                Key = key
            };

            GetObjectResponse response;

            try
            {
                response = await s3Client.GetObjectAsync(getRequest);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If thumbnail not found, try original file
                if (fileName != originalFileName)
                {
                    key = $"{folderPath}/{originalFileName}";
                    getRequest.Key = key;
                    needReupload = true;

                    try
                    {
                        response = await s3Client.GetObjectAsync(getRequest);
                    }
                    catch (AmazonS3Exception)
                    {
                        logger.LogInformation("File not found in S3 storage: {bucketName}/{key}",
                                              cfg.BucketName,
                                              key);
                        return Results.NotFound();
                    }
                }
                else
                {
                    logger.LogInformation("File not found in S3 storage: {bucketName}/{key}",
                                          cfg.BucketName,
                                          key);
                    return Results.NotFound();
                }
            }

            var swEnd = Stopwatch.GetTimestamp();

            logger.LogInformation("Downloaded {contentLength} bytes from S3 storage {bucketName}/{key} in {elapsed}",
                                  response.ContentLength,
                                  cfg.BucketName,
                                  key,
                                  Stopwatch.GetElapsedTime(swStart, swEnd));

            // If we need to resize and reupload thumbnail
            if (needResize && needReupload)
            {
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                await uploadService.Upload(memoryStream,
                                           new UploadRequestModel
                                           {
                                               ContentType = response.Headers.ContentType,
                                               Path        = folderPath,
                                               ThumbnailGenerateRequests =
                                               [
                                                   new ThumbnailGenerateRequestModel
                                                   {
                                                       ThumbnailWidth  = width ?? 0,
                                                       ThumbnailHeight = height ?? 0
                                                   }
                                               ],
                                               OnlyUploadThumbnails = true,
                                               UploadInBackground   = true
                                           },
                                           originalFileName,
                                           null);

                memoryStream.Position = 0;
                return Results.File(memoryStream, response.Headers.ContentType);
            }

            return Results.Stream(response.ResponseStream, response.Headers.ContentType);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex,
                            "S3 Error while downloading from {bucketName}/{key}: {errorCode} - {message}",
                            cfg.BucketName,
                            key,
                            ex.ErrorCode,
                            ex.Message);

            return Results.Problem($"Error accessing S3 storage: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Error while downloading from S3 storage {bucketName}/{key}",
                            cfg.BucketName,
                            key);

            return Results.Problem("Error retrieving file from S3 storage");
        }
    }
}