using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.S3Storage.Internal;
using DotNetBrightener.UploadService.S3Storage.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.UploadService.S3Storage.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers S3-compatible storage upload service provider
    /// </summary>
    /// <param name="builder">The upload service configuration builder</param>
    /// <param name="configuration">The application configuration</param>
    public static void RegisterS3StorageUploadService(this UploadServiceConfigurationBuilder builder,
                                                      IConfiguration                         configuration)
    {
        builder.AddUploadServiceProvider<S3StorageUploadServiceProvider>();
        builder.ServiceCollection.AddScoped<IS3StorageUploadBackgroundTask, S3StorageUploadBackgroundTask>();

        var configSectionName = nameof(S3StorageConfiguration);
        builder.ServiceCollection.Configure<S3StorageConfiguration>(configuration.GetSection(configSectionName));
    }
}