# S3 Storage Upload Service - Quick Start Guide

## Installation

```bash
dotnet add package DotNetBrightener.UploadService.S3Storage
```

## Basic Setup (3 Steps)

### Step 1: Add Configuration

Add to your `appsettings.json`:

```json
{
  "S3StorageConfiguration": {
    "Provider": "AWS",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Region": "us-east-1",
    "BucketName": "your-bucket-name",
    "ServiceUrl": "https://s3.us-east-1.amazonaws.com",
    "ForcePathStyle": false
  }
}
```

### Step 2: Register Services

In `Program.cs`:

```csharp
using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.S3Storage.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterSimpleUploadService()
                .RegisterS3StorageUploadService(builder.Configuration);

var app = builder.Build();

app.MapS3StorageFileEndpoint();

app.Run();
```

### Step 3: Upload Files

```csharp
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        
        var result = await _uploadService.Upload(
            stream,
            new UploadRequestModel
            {
                ContentType = file.ContentType,
                Path = "uploads"
            },
            file.FileName,
            null
        );

        return Ok(result);
    }
}
```

## Provider-Specific Quick Configs

### AWS S3
```json
{
  "S3StorageConfiguration": {
    "Provider": "AWS",
    "Region": "us-east-1",
    "BucketName": "my-bucket",
    "ServiceUrl": "https://s3.us-east-1.amazonaws.com"
  }
}
```

### Digital Ocean Spaces
```json
{
  "S3StorageConfiguration": {
    "Provider": "DigitalOcean",
    "Region": "nyc3",
    "BucketName": "my-space",
    "ServiceUrl": "https://nyc3.digitaloceanspaces.com"
  }
}
```

### Wasabi
```json
{
  "S3StorageConfiguration": {
    "Provider": "Wasabi",
    "Region": "us-east-1",
    "BucketName": "my-bucket",
    "ServiceUrl": "https://s3.us-east-1.wasabisys.com"
  }
}
```

### MinIO (Self-Hosted)
```json
{
  "S3StorageConfiguration": {
    "Provider": "MinIO",
    "Region": "us-east-1",
    "BucketName": "my-bucket",
    "ServiceUrl": "https://minio.example.com:9000",
    "ForcePathStyle": true
  }
}
```

### Backblaze B2
```json
{
  "S3StorageConfiguration": {
    "Provider": "Backblaze",
    "Region": "us-west-002",
    "BucketName": "my-bucket",
    "ServiceUrl": "https://s3.us-west-002.backblazeb2.com"
  }
}
```

## Common Features

### Upload with Thumbnails

```csharp
var result = await _uploadService.Upload(
    stream,
    new UploadRequestModel
    {
        ContentType = file.ContentType,
        Path = "images",
        ThumbnailGenerateRequests = new[]
        {
            new ThumbnailGenerateRequestModel
            {
                ThumbnailWidth = 200,
                ThumbnailHeight = 200
            },
            new ThumbnailGenerateRequestModel
            {
                ThumbnailWidth = 800,
                ThumbnailHeight = 600
            }
        }
    },
    file.FileName,
    null
);
```

### Retrieve Files

Access uploaded files via the configured endpoint:

```
GET /s3_file/{folder}/{filename}
GET /s3_file/{folder}/{filename}?width=200&height=200
```

Example:
```
GET /s3_file/images/photo.jpg
GET /s3_file/images/photo.jpg?width=200&height=200
```

## Configuration Options

| Property | Required | Default | Description |
|----------|----------|---------|-------------|
| `Provider` | No | - | Provider type (AWS, DigitalOcean, Wasabi, MinIO, Backblaze, Custom) |
| `AccessKey` | Yes | - | S3 Access Key ID |
| `SecretKey` | Yes | - | S3 Secret Access Key |
| `Region` | Yes | - | S3 Region |
| `BucketName` | Yes | - | S3 Bucket name |
| `ServiceUrl` | Yes | - | S3 Service endpoint URL |
| `RetrieveFileEndpoint` | No | `/s3_file` | HTTP endpoint for file retrieval |
| `UploadInBackground` | No | `false` | Upload files in background |
| `TempDownloadFolder` | No | `tmp_download` | Local cache folder |
| `UseGuidForFileName` | No | `false` | Use GUID for file names |
| `CacheExpiration` | No | `06:00:00` | Cache expiration time |
| `ForcePathStyle` | No | `false` | Force path-style bucket access (required for MinIO) |

## Next Steps

- Read the [full documentation](readme.md) for advanced features
- Learn about [switching providers](readme.md#switching-between-providers)
- Explore [cost comparisons](readme.md#cost-comparison)
- Check [troubleshooting guide](readme.md#troubleshooting)

