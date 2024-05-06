using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.AzureBlobStorage.Internal;
using DotNetBrightener.UploadService.AzureBlobStorage.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.UploadService.AzureBlobStorage.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterAzureUploadService(this UploadServiceConfigurationBuilder builder,
                                                  IConfiguration                         configuration)
    {
        builder.AddUploadServiceProvider<AzureBlobStorageUploadServiceProvider>();
        builder.ServiceCollection.AddScoped<IAzureUploadBackgroundTask, AzureUploadBackgroundTask>();

        var configSectionName = nameof(AzureBlobStorageConfiguration);
        builder.ServiceCollection.Configure<AzureBlobStorageConfiguration>(configuration.GetSection(configSectionName));
    }
}