using System.Diagnostics;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.UploadService.S3Storage.Internal;

public interface IS3StorageUploadBackgroundTask
{
    /// <summary>
    ///     Performs upload the local file to the S3-compatible storage
    /// </summary>
    /// <param name="localFilePath">
    ///     The path to the local file
    /// </param>
    /// <param name="folderPath">
    ///     The folder path in the S3 bucket to store the file
    /// </param>
    /// <param name="fileName">
    ///     The name of the file in the bucket
    /// </param>
    /// <param name="contentType">
    ///     The content type of the file
    /// </param>
    /// <returns></returns>

    Task UploadLocalFile(string localFilePath, string folderPath, string fileName, string contentType);

    /// <summary>
    ///     Performs upload the given stream to the S3-compatible storage
    /// </summary>
    /// <param name="uploadStream">
    ///     The file stream to upload
    /// </param>
    /// <param name="folderPath">
    ///     The folder path in the S3 bucket to store the file
    /// </param>
    /// <param name="fileName">
    ///     The name of the file in the bucket
    /// </param>
    /// <param name="contentType">
    ///     The content type of the file
    /// </param>
    /// <returns></returns>
    Task UploadFromStream(Stream uploadStream, string folderPath, string fileName, string contentType);
}

internal class S3StorageUploadBackgroundTask(
    IOptions<S3StorageConfiguration> options,
    ILogger<S3StorageUploadBackgroundTask>  logger) : IS3StorageUploadBackgroundTask
{
    private readonly S3StorageConfiguration _configuration = options.Value;
    private readonly Lazy<IAmazonS3> _s3Client = new(() => CreateS3Client(options.Value));

    private static IAmazonS3 CreateS3Client(S3StorageConfiguration config)
    {
        var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = config.ServiceUrl,
            ForcePathStyle = config.ForcePathStyle
        };

        return new AmazonS3Client(credentials, s3Config);
    }

    public async Task UploadLocalFile(string localFilePath,
                                      string folderPath,
                                      string fileName,
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

            logger.LogInformation("Uploading file {localFilePath} to S3 storage", localFilePath);
            await UploadFromStream(stream, folderPath, fileName, contentType);
        }

        var swEnd = Stopwatch.GetTimestamp();

        logger.LogInformation("File {localFilePath} uploaded to S3 storage in {elapsedTime}",
                              localFilePath,
                              Stopwatch.GetElapsedTime(sw, swEnd));

        // Clean up local file after successful upload
        try
        {
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
                logger.LogInformation("Deleted local temp file {localFilePath}", localFilePath);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete local temp file {localFilePath}", localFilePath);
        }
    }

    public async Task UploadFromStream(Stream uploadStream,
                                       string folderPath,
                                       string fileName,
                                       string contentType)
    {
        var key = string.IsNullOrEmpty(folderPath) ? fileName : $"{folderPath}/{fileName}";

        var sw = Stopwatch.GetTimestamp();

        logger.LogInformation("Uploading stream to S3 storage: {bucketName}/{key}",
                              _configuration.BucketName,
                              key);

        try
        {
            if (uploadStream.Position > 0)
                uploadStream.Position = 0; // reset stream position
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error occurred when trying to reset stream.");
        }

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _configuration.BucketName,
                Key = key,
                InputStream = uploadStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead // Make files publicly readable
            };

            var response = await _s3Client.Value.PutObjectAsync(putRequest);

            var swEnd = Stopwatch.GetTimestamp();

            logger.LogInformation("Stream uploaded to S3 storage {bucketName}/{key} in {elapsedTime}. ETag: {etag}",
                                  _configuration.BucketName,
                                  key,
                                  Stopwatch.GetElapsedTime(sw, swEnd),
                                  response.ETag);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex,
                            "S3 Error while uploading to {bucketName}/{key}: {errorCode} - {message}",
                            _configuration.BucketName,
                            key,
                            ex.ErrorCode,
                            ex.Message);

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Error while uploading to S3 storage {bucketName}/{key}",
                            _configuration.BucketName,
                            key);

            throw;
        }
    }
}