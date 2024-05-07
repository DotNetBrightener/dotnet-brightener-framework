using Azure.Storage.Blobs;
using DotNetBrightener.SimpleUploadService.Models;
using DotNetBrightener.SimpleUploadService.Services;
using DotNetBrightener.UploadService.AzureBlobStorage.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;

namespace DotNetBrightener.UploadService.AzureBlobStorage.Endpoints;

public static class AzureFileEndpoints
{
    /// <summary>
    ///     Registers the endpoint for retrieving files from Azure Blob Storage
    /// </summary>
    /// <param name="endpoints">
    ///     The <see cref="IEndpointRouteBuilder"/>
    /// </param>
    public static void MapAzureBlobStorageFileEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var configuration = endpoints.ServiceProvider.GetRequiredService<IOptions<AzureBlobStorageConfiguration>>();

        var azureFileEndpointGroup = endpoints.MapGroup(configuration.Value.RetrieveFileEndpoint);

        azureFileEndpointGroup.MapGet("{*filePath}",
                                      async (string                                  filePath,
                                             IOptions<AzureBlobStorageConfiguration> azureBlobStorageConfiguration,
                                             IHostEnvironment                        environment,
                                             ILoggerFactory                          loggerFactory,
                                             IUploadService                          uploadService,
                                             IContentTypeProvider contentTypeProvider,
                                             [FromQuery]
                                             int? width = null,
                                             [FromQuery]
                                             int? height = null) =>
                                      {
                                          var logger = loggerFactory.CreateLogger(typeof(AzureFileEndpoints));

                                          return await DownloadFileFromBlobStorage(filePath,
                                                                                   azureBlobStorageConfiguration.Value,
                                                                                   environment,
                                                                                   uploadService,
                                                                                   contentTypeProvider,
                                                                                   logger,
                                                                                   width,
                                                                                   height);
                                      });
    }

    private static async Task<IResult> DownloadFileFromBlobStorage(string                        filePath,
                                                                   AzureBlobStorageConfiguration cfg,
                                                                   IHostEnvironment              environment,
                                                                   IUploadService                uploadService,
                                                                   IContentTypeProvider          contentTypeProvider,
                                                                   ILogger                       logger,
                                                                   int?                          width  = null,
                                                                   int?                          height = null)
    {
        var blobSegments = filePath.Split("/", StringSplitOptions.RemoveEmptyEntries);

        if (blobSegments.Length != 2)
            return null;

        var blobContainer    = blobSegments[0];
        var blobName         = blobSegments[1];
        var originalBlobName = blobName;

        var needResize   = width != null || height != null;
        var needReupload = false;

        if (needResize)
        {
            blobName = AzureThumbnailNameUtils.GetThumbnailFileName(originalBlobName, width ?? 0, height ?? 0);
        }

        var tmpLocalFile = Path.Combine(environment.ContentRootPath, cfg.TempDownloadFolder, blobName);

        if (File.Exists(tmpLocalFile))
        {
            contentTypeProvider.TryGetContentType(tmpLocalFile, out var contentType);
            logger.LogInformation("Response from local file {blocName}", blobName);

            return Results.File(tmpLocalFile, contentType ?? "application/octet-stream");
        }

        if (blobName != originalBlobName)
        {
            tmpLocalFile = Path.Combine(environment.ContentRootPath, cfg.TempDownloadFolder, originalBlobName);

            if (File.Exists(tmpLocalFile))
            {
                contentTypeProvider.TryGetContentType(tmpLocalFile, out var contentType);

                logger.LogInformation("Response from local file {blocName}", originalBlobName);

                return Results.File(tmpLocalFile, contentType ?? "application/octet-stream");
            }
        }

        logger.LogInformation("No local file available for requested blob {blob}. Try fetching from Azure Blob Storage.",
                              originalBlobName);

        var container = new BlobContainerClient(cfg.ConnectionString, blobContainer);

        if (!await container.ExistsAsync())
        {
            logger.LogInformation("No container {blobContainer} found in given blob storage.", blobContainer);

            return Results.NotFound();
        }

        BlobClient blob = container.GetBlobClient(blobName);

        if (!await blob.ExistsAsync())
        {
            if (blobName != originalBlobName)
            {
                blob         = container.GetBlobClient(originalBlobName);
                needReupload = true;
            }
            else
            {
                logger.LogInformation("No blob with name ({blobContainer}/{blobName}) found in given blob storage.",
                                      blobContainer,
                                      blobName);

                return Results.NotFound();
            }
        }

        if (!await blob.ExistsAsync())
        {
            logger.LogInformation("No blob with name ({blobContainer}/{blobName}) found in given blob storage.",
                                  blobContainer,
                                  blobName);

            return Results.NotFound();
        }

        var swStart = Stopwatch.GetTimestamp();

        var streamResponse = await blob.DownloadContentAsync();

        var swEnd = Stopwatch.GetTimestamp();

        if (streamResponse.Value.Content == null)
        {
            logger.LogInformation("No content found for blob path {blobName} in {elapsed}",
                                  blobName,
                                  Stopwatch.GetElapsedTime(swStart, swEnd));

            return Results.NotFound();
        }

        if (needResize && needReupload)
        {
            await uploadService.Upload(streamResponse.Value.Content.ToStream(),
                                       new UploadRequestModel
                                       {
                                           ContentType = streamResponse.Value.Details.ContentType,
                                           Path        = blobContainer,
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
                                       originalBlobName,
                                       null);
        }

        return Results.File(streamResponse.Value.Content.ToStream(), streamResponse.Value.Details.ContentType);

    }
}