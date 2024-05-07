using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.ImageOptimizer;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class UploadServiceBuilderExtensions
{
    /// <summary>
    ///     Adds the Image Resizer using ImageMagick to the <see cref="UploadServiceConfigurationBuilder"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static UploadServiceConfigurationBuilder
        UseImageMagickOptimizer(this UploadServiceConfigurationBuilder builder)
    {
        builder.UseImageResizer<ImageMagickOptimizer>();

        return builder;
    }
}