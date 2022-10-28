# Simple Upload API for ASP.NET Application

&copy; 2022 DotNet Brightener

## Installation

Run this in command line:

``` powershell/cmd/bash/shell
dotnet add package DotNetBrightener.SimpleUploadService
```

Or add the following to `.csproj` file

```xml
<PackageReference Include="DotNetBrightener.SimpleUploadService" Version="2022.10.0" />
```

## Usage

### 1. Register the service

```cs 
serviceCollection.RegisterSimpleUploadService(builder => {
	// configure your upload configuration here
});
```

### 2. Upload Path

Default upload folder is `{environment.ContentRootPath}/Media`. For example, if you deploy the application in `/app` of the computer, the upload folder will be `/app/Media`.

You can replace the default upload folder by implementing `IUploadFolderPathResolver` interface and provide your own logic of determining where to store the uploaded file.

```cs
public class TenantBasedUploadPathResolver : IUploadFolderResolver 
{
    public string UploadRootPath { get; }

    public TenantBasedUploadPathResolver(IHostEnvironment hostEnvironment)
    {
        this.UploadRootPath = hostEnvironment.ContentRootPath;
    }

    public Task<string> ResolveUploadPath(string uploadPath)
    {
        return Task.FromResult(uploadPath.Replace('/', Path.DirectorySeparatorChar)
                                         .Replace('\\', Path.DirectorySeparatorChar));
    }

}
```

### Resizing image when upload

Implement your own logic for resizing photos by deriving the `IImageResizer` interface. Then you need to register it by 

```cs

```

## Extend API with `IUploadServiceProvider`

