# S3-Compatible Storage Upload Service Provider

This package provides a universal S3-compatible storage upload service provider for the DotNetBrightener Upload Service framework. It works with **AWS S3**, **Digital Ocean Spaces**, **Wasabi**, **MinIO**, **Backblaze B2**, and any other S3-compatible storage service.

## Features

- **Universal S3 Compatibility**: Works with any S3-compatible storage provider
- **Multiple Provider Support**: Pre-configured for AWS S3, Digital Ocean Spaces, Wasabi, MinIO, Backblaze B2
- **Background Upload Support**: Asynchronous file uploads to prevent blocking client requests
- **Thumbnail Generation**: Automatic thumbnail creation and caching
- **Local Caching**: Configurable file caching to reduce API calls
- **GUID File Naming**: Optional GUID-based file naming for uniqueness
- **Public Access**: Files are uploaded with public read access by default
- **Flexible Configuration**: Easy switching between providers with minimal configuration changes

## Installation

```bash
dotnet add package DotNetBrightener.UploadService.S3Storage
```

## Supported Providers

| Provider | Status | Notes |
|----------|--------|-------|
| AWS S3 | ✅ Fully Supported | Native AWS S3 service |
| Digital Ocean Spaces | ✅ Fully Supported | S3-compatible object storage |
| Wasabi | ✅ Fully Supported | Hot cloud storage |
| MinIO | ✅ Fully Supported | Self-hosted object storage |
| Backblaze B2 | ✅ Fully Supported | S3-compatible API |
| Custom S3 | ✅ Fully Supported | Any S3-compatible service |

## Configuration

### Base Configuration Structure

Add the following configuration to your `appsettings.json`:

```json
{
  "S3StorageConfiguration": {
    "Provider": "AWS",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Region": "us-east-1",
    "BucketName": "your-bucket-name",
    "ServiceUrl": "https://s3.us-east-1.amazonaws.com",
    "RetrieveFileEndpoint": "/s3_file",
    "UploadInBackground": true,
    "TempDownloadFolder": "tmp_download",
    "UseGuidForFileName": false,
    "CacheExpiration": "06:00:00",
    "ForcePathStyle": false
  }
}
```

### Configuration Properties

- **Provider**: (Optional) The S3 provider type for documentation purposes. Values: `AWS`, `DigitalOcean`, `Wasabi`, `MinIO`, `Backblaze`, `Custom`
- **AccessKey**: S3 Access Key ID
- **SecretKey**: S3 Secret Access Key
- **Region**: S3 Region identifier
- **BucketName**: S3 Bucket name
- **ServiceUrl**: S3 Service endpoint URL (see provider-specific examples below)
- **RetrieveFileEndpoint**: HTTP endpoint path for retrieving files (default: "/s3_file")
- **UploadInBackground**: Whether to upload files in background (default: false)
- **TempDownloadFolder**: Local folder for temporary file storage (default: "tmp_download")
- **UseGuidForFileName**: Use GUID for file names to ensure uniqueness (default: false)
- **CacheExpiration**: How long to cache files locally (default: 6 hours)
- **ForcePathStyle**: Force path-style bucket access (required for MinIO and some providers)

### Provider-Specific Configuration Examples

#### AWS S3

```json
{
  "S3StorageConfiguration": {
    "Provider": "AWS",
    "AccessKey": "AKIAIOSFODNN7EXAMPLE",
    "SecretKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
    "Region": "us-east-1",
    "BucketName": "my-app-bucket",
    "ServiceUrl": "https://s3.us-east-1.amazonaws.com",
    "ForcePathStyle": false
  }
}
```

**Note**: For AWS S3, you can omit `ServiceUrl` and it will be automatically constructed from the region.

#### Digital Ocean Spaces

```json
{
  "S3StorageConfiguration": {
    "Provider": "DigitalOcean",
    "AccessKey": "DO00ABC123XYZ",
    "SecretKey": "your-secret-key-here",
    "Region": "nyc3",
    "BucketName": "my-app-storage",
    "ServiceUrl": "https://nyc3.digitaloceanspaces.com",
    "ForcePathStyle": false
  }
}
```

**Available Regions**: `nyc3`, `sfo3`, `sgp1`, `ams3`, `fra1`, `blr1`, `syd1`

#### Wasabi

```json
{
  "S3StorageConfiguration": {
    "Provider": "Wasabi",
    "AccessKey": "WASABI-ACCESS-KEY",
    "SecretKey": "wasabi-secret-key",
    "Region": "us-east-1",
    "BucketName": "my-wasabi-bucket",
    "ServiceUrl": "https://s3.us-east-1.wasabisys.com",
    "ForcePathStyle": false
  }
}
```

**Available Regions**: `us-east-1`, `us-east-2`, `us-west-1`, `eu-central-1`, `ap-northeast-1`, `ap-northeast-2`

#### MinIO (Self-Hosted)

```json
{
  "S3StorageConfiguration": {
    "Provider": "MinIO",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Region": "us-east-1",
    "BucketName": "my-bucket",
    "ServiceUrl": "https://minio.example.com:9000",
    "ForcePathStyle": true
  }
}
```

**Important**: MinIO requires `ForcePathStyle: true`

#### Backblaze B2

```json
{
  "S3StorageConfiguration": {
    "Provider": "Backblaze",
    "AccessKey": "your-key-id",
    "SecretKey": "your-application-key",
    "Region": "us-west-002",
    "BucketName": "my-b2-bucket",
    "ServiceUrl": "https://s3.us-west-002.backblazeb2.com",
    "ForcePathStyle": false
  }
}
```

## Usage

### 1. Register the Service

In your `Program.cs` or `Startup.cs`:

```csharp
using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.S3Storage.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register the upload service
builder.Services.RegisterSimpleUploadService()
                .RegisterS3StorageUploadService(builder.Configuration);

var app = builder.Build();

// Map the file endpoint
app.MapS3StorageFileEndpoint();

app.Run();
```

### 2. Upload Files

```csharp
public class FileUploadController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public FileUploadController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        using var stream = file.OpenReadStream();

        var result = await _uploadService.Upload(
            stream,
            new UploadRequestModel
            {
                Path = "images",
                ContentType = file.ContentType,
                UploadInBackground = true,
                ThumbnailGenerateRequests = new[]
                {
                    new ThumbnailGenerateRequestModel
                    {
                        ThumbnailWidth = 200,
                        ThumbnailHeight = 200
                    }
                }
            },
            file.FileName,
            Request.GetBaseUrl()
        );

        return Ok(result);
    }
}
```

### 3. Retrieve Files

Files can be accessed via the configured endpoint:

```
GET /s3_file/{folder}/{filename}
GET /s3_file/{folder}/{filename}?width=200&height=200
```

## Architecture

The S3 Storage provider follows the same architectural pattern as other upload service providers:

- **S3StorageUploadServiceProvider**: Main upload service provider implementing `IUploadServiceProvider`
- **S3StorageUploadBackgroundTask**: Handles actual file uploads to S3-compatible storage using AWS S3 SDK
- **S3StorageFileEndpoints**: Provides HTTP endpoints for file retrieval with caching support
- **ThumbnailNameUtils**: Utility for generating thumbnail file names
- **S3StorageConfiguration**: Configuration class with provider enum and flexible settings

## How It Works

1. **Upload Flow**:
   - Client uploads file to your API
   - File is optionally saved to local temp folder
   - Upload task is scheduled (if background upload is enabled) or executed immediately
   - File is uploaded to S3-compatible storage using S3 API
   - Thumbnails are generated and uploaded if requested
   - Local temp file is cleaned up after successful upload

2. **Download Flow**:
   - Client requests file via endpoint
   - System checks local cache first
   - If not cached or cache expired, fetches from S3 storage
   - File is streamed to client
   - Thumbnails are generated on-demand if requested but not available

## Switching Between Providers

One of the key benefits of this package is the ability to easily switch between S3-compatible providers with minimal configuration changes:

### Example: Switching from AWS S3 to Digital Ocean Spaces

**Before (AWS S3):**
```json
{
  "S3StorageConfiguration": {
    "Provider": "AWS",
    "Region": "us-east-1",
    "BucketName": "my-aws-bucket",
    "ServiceUrl": "https://s3.us-east-1.amazonaws.com"
  }
}
```

**After (Digital Ocean Spaces):**
```json
{
  "S3StorageConfiguration": {
    "Provider": "DigitalOcean",
    "Region": "nyc3",
    "BucketName": "my-do-bucket",
    "ServiceUrl": "https://nyc3.digitaloceanspaces.com"
  }
}
```

**No code changes required!** Just update your configuration and credentials.

## Benefits of S3-Compatible Storage

### Cost Comparison

| Provider | Storage Cost (per GB/month) | Bandwidth Cost (per GB) | Notes |
|----------|----------------------------|------------------------|-------|
| AWS S3 | $0.023 | $0.09 | Standard tier |
| Digital Ocean Spaces | $0.02 | Free (1TB included) | Flat $5/month minimum |
| Wasabi | $0.0059 | Free | No egress fees |
| Backblaze B2 | $0.005 | $0.01 (after 3x storage) | Very cost-effective |
| MinIO | Self-hosted | Self-hosted | Full control |

### Why Use This Package?

1. **Vendor Independence**: Easily switch between providers without code changes
2. **Cost Optimization**: Choose the most cost-effective provider for your needs
3. **Multi-Cloud Strategy**: Use different providers for different environments
4. **Self-Hosting Option**: Use MinIO for complete control and privacy
5. **Disaster Recovery**: Easily replicate to multiple providers

## Requirements

- .NET 10.0 or later
- S3-compatible storage account with access keys
- AWSSDK.S3 package (automatically installed)

## Troubleshooting

### Common Issues

**Issue**: "Access Denied" errors
- **Solution**: Verify your access key and secret key are correct
- **Solution**: Ensure the bucket exists and your credentials have proper permissions

**Issue**: "Endpoint not found" errors
- **Solution**: Verify the `ServiceUrl` is correct for your provider
- **Solution**: For MinIO, ensure `ForcePathStyle: true` is set

**Issue**: Files not uploading
- **Solution**: Check bucket permissions (should allow PutObject)
- **Solution**: Verify network connectivity to the S3 endpoint

**Issue**: Thumbnails not generating
- **Solution**: Ensure image resizer is registered (e.g., SkiaSharp)
- **Solution**: Check that the uploaded file is a valid image format

## License

This package is part of the DotNetBrightener framework.
