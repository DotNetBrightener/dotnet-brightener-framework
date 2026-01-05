namespace DotNetBrightener.UploadService.S3Storage;

/// <summary>
/// S3-compatible storage provider types
/// </summary>
public enum S3StorageProvider
{
    /// <summary>
    /// Amazon Web Services S3
    /// </summary>
    AWS,

    /// <summary>
    /// Digital Ocean Spaces
    /// </summary>
    DigitalOcean,

    /// <summary>
    /// Wasabi Hot Cloud Storage
    /// </summary>
    Wasabi,

    /// <summary>
    /// MinIO Object Storage
    /// </summary>
    MinIO,

    /// <summary>
    /// Backblaze B2 Cloud Storage
    /// </summary>
    Backblaze,

    /// <summary>
    /// Custom S3-compatible provider
    /// </summary>
    Custom
}

public class S3StorageConfiguration
{
    public const string DefaultDownloadFolder = "tmp_download";

    public const string DefaultFileEndpoint = "/s3_file";

    /// <summary>
    /// S3-compatible storage provider type (optional, for documentation purposes)
    /// </summary>
    public S3StorageProvider? Provider { get; set; }

    /// <summary>
    /// S3 Access Key ID
    /// </summary>
    public string AccessKey { get; set; }

    /// <summary>
    /// S3 Secret Access Key
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// S3 Region (e.g., "us-east-1" for AWS, "nyc3" for Digital Ocean)
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// S3 Bucket Name
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// S3 Service Endpoint URL
    /// <para>Examples:</para>
    /// <para>- AWS S3: Leave empty or use "https://s3.{region}.amazonaws.com"</para>
    /// <para>- Digital Ocean: "https://{region}.digitaloceanspaces.com"</para>
    /// <para>- Wasabi: "https://s3.{region}.wasabisys.com"</para>
    /// <para>- MinIO: "https://your-minio-server:9000"</para>
    /// <para>- Backblaze: "https://s3.{region}.backblazeb2.com"</para>
    /// </summary>
    public string ServiceUrl { get; set; }

    /// <summary>
    /// HTTP endpoint path for retrieving files (default: "/s3_file")
    /// </summary>
    public string RetrieveFileEndpoint { get; set; } = DefaultFileEndpoint;

    /// <summary>
    /// Whether to upload files in background using Hangfire
    /// </summary>
    public bool UploadInBackground { get; set; }

    /// <summary>
    /// Local temporary folder for storing files before background upload
    /// </summary>
    public string TempDownloadFolder { get; set; } = DefaultDownloadFolder;

    /// <summary>
    /// Use GUID for file names to ensure uniqueness
    /// </summary>
    public bool UseGuidForFileName { get; set; }

    /// <summary>
    /// How long to cache downloaded files locally
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Force path-style bucket access (bucket.s3.amazonaws.com vs s3.amazonaws.com/bucket)
    /// Required for MinIO and some other providers
    /// </summary>
    public bool ForcePathStyle { get; set; }
}