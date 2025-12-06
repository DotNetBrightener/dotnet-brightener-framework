using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.UploadService.ImageOptimizer;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class UploadServiceBuilderExtensions
{
	/// <summary>
	///     Adds the Image Resizer using SkiaSharp to the <see cref="UploadServiceConfigurationBuilder"/>
	/// </summary>
	/// <param name="builder">The upload service configuration builder</param>
	/// <returns>The same builder instance for method chaining</returns>
	public static UploadServiceConfigurationBuilder
		UseSkiaSharpOptimizer(this UploadServiceConfigurationBuilder builder)
	{
		builder.UseImageResizer<SkiaSharpImageOptimizer>();
		return builder;
	}

}