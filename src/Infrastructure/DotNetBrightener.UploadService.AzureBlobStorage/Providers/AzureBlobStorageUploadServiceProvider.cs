using Azure.Storage.Blobs;
using DotNetBrightener.SimpleUploadService;
using DotNetBrightener.SimpleUploadService.IO;
using DotNetBrightener.SimpleUploadService.Models;
using DotNetBrightener.SimpleUploadService.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.UploadService.AzureBlobStorage.Providers;

public class AzureBlobStorageUploadServiceProvider: IUploadServiceProvider
{
    public int Priority => 200;

    public IImageResizer ImageResizer { get; }

    public AzureBlobStorageUploadServiceProvider(IMediaFileProvider                    mediaFileStore,
                                                 IContentTypeProvider                  contentTypeProvider,
                                                 ILogger<DefaultUploadServiceProvider> logger,
                                                 IImageResizer                         imageResizer)
    {
        ImageResizer = imageResizer;
    }

    public async Task<FileObjectModel> ProcessUpload(Stream fileUploadStream, UploadRequestModel uploadRequestModel, string fileName, string baseUrl)
    {
        throw new System.NotImplementedException();
        var fileObjectModel = new FileObjectModel
        {
        };

        string connectionString = "<connection_string>";
        
        string blobName         = "sample-blob";
        string filePath         = "sample-file";

        // Get a reference to a container named "sample-container" and then create it
        var container = new BlobContainerClient(connectionString, uploadRequestModel.Path);
        await container.CreateAsync();
        
        var blob = container.GetBlobClient(fileName);
        var uploadResponse = await blob.UploadAsync(fileUploadStream);

        //fileObjectModel.AbsoluteUrl = blob.GenerateSasUri(BlobSasPermissions.Read, ).Value.

        return fileObjectModel;
    }

    public Task<string>                GenerateSecuredUrl(string absoluteUrl)
    {
        throw new System.NotImplementedException();
    }
}