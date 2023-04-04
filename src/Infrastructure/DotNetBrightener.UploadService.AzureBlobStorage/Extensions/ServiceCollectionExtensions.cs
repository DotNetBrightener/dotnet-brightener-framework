using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.AzureBlobStorage.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.UploadService.AzureBlobStorage.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterAzureUploadService(this UploadServiceConfigurationBuilder builder)
    {
        builder.AddUploadServiceProvider<AzureBlobStorageUploadServiceProvider>();
    }
}