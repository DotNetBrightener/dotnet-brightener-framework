using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace DotNetBrightener.UploadService.AzureBlobStorage.Internal;

public interface IAzureUploadBackgroundTask
{
    /// <summary>
    ///     Performs upload the local file to the Azure Blob Storage
    /// </summary>
    /// <param name="localFilePath">
    ///     The path to the local file
    /// </param>
    /// <param name="containerName">
    ///     The container name in the Azure Blob Storage to store the file
    /// </param>
    /// <param name="blobName">
    ///     The name of the blob in the container
    /// </param>
    /// <param name="contentType">
    ///     The content type of the file
    /// </param>
    /// <returns></returns>
    
    Task UploadLocalFile(string localFilePath, string containerName, string blobName, string contentType);

    /// <summary>
    ///     Performs upload the given stream to the Azure Blob Storage
    /// </summary>
    /// <param name="uploadStream">
    ///     The file stream to upload
    /// </param>
    /// <param name="containerName">
    ///     The container name in the Azure Blob Storage to store the file
    /// </param>
    /// <param name="blobName">
    ///     The name of the blob in the container
    /// </param>
    /// <param name="contentType">
    ///     The content type of the file
    /// </param>
    /// <returns></returns>
    Task UploadFromStream(Stream uploadStream, string containerName, string blobName, string contentType);
}

internal class AzureUploadBackgroundTask(
    IOptions<AzureBlobStorageConfiguration> options,
    ILogger<AzureUploadBackgroundTask>      logger) : IAzureUploadBackgroundTask
{
    private readonly AzureBlobStorageConfiguration _azureBlobStorageConfiguration = options.Value;

    public async Task UploadLocalFile(string localFilePath,
                                      string containerName,
                                      string blobName,
                                      string contentType)
    {
        var sw = Stopwatch.GetTimestamp();

        await using (var stream = File.OpenRead(localFilePath))
        {
            if (stream.Length == 0)
            {
                logger.LogWarning("File {localFilePath} is empty. Upload cancelled.", localFilePath);

                return;
            }

            logger.LogInformation("Uploading file {localFilePath} to blob storage", localFilePath);
            await UploadFromStream(stream, containerName, blobName, contentType);
        }

        var swEnd = Stopwatch.GetTimestamp();

        logger.LogInformation("File {localFilePath} uploaded to blob storage in {elapsedTime}",
                              localFilePath,
                              Stopwatch.GetElapsedTime(sw, swEnd));
    }

    public async Task UploadFromStream(Stream uploadStream,
                                       string containerName,
                                       string blobName,
                                       string contentType)
    {
        var container = new BlobContainerClient(_azureBlobStorageConfiguration.ConnectionString, containerName);

        try
        {
            await container.CreateIfNotExistsAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                            "Error while trying to create container {containerName}",
                            containerName);

            throw;
        }

        var blob = container.GetBlobClient(blobName);

        var sw = Stopwatch.GetTimestamp();

        logger.LogInformation("Uploading stream to {blobUri}", blob.Uri);

        try
        {
            if (uploadStream.Position > 0)
                uploadStream.Position = 0; // reset stream position
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error occurred when trying to reset stream.");
        }

        await blob.UploadAsync(uploadStream,
                               new BlobHttpHeaders
                               {
                                   ContentType = contentType
                               });

        var swEnd = Stopwatch.GetTimestamp();

        logger.LogInformation("Stream uploaded to {blobUri} in {elapsedTime}",
                              blob.Uri,
                              Stopwatch.GetElapsedTime(sw, swEnd));
    }
}