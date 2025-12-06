using DotNetBrightener.SimpleUploadService;
using SkiaSharp;

namespace DotNetBrightener.UploadService.ImageOptimizer;

/// <summary>
///     Image optimizer implementation using SkiaSharp library
/// </summary>
public class SkiaSharpImageOptimizer : IImageResizer
{
    /// <summary>
    ///     Resizes an image from the input stream to the specified dimensions
    /// </summary>
    /// <param name="inputStream">The input image stream</param>
    /// <param name="newWidth">The target width (absolute value will be used)</param>
    /// <param name="newHeight">The target height (absolute value will be used)</param>
    /// <returns>A new stream containing the resized image in PNG format</returns>
    public Stream ResizeImageFromStream(Stream inputStream, int newWidth, int newHeight)
    {
        var memoryStream = new MemoryStream();

        // Ensure positive dimensions
        var scaledWidth  = Math.Abs(newWidth);
        var scaledHeight = Math.Abs(newHeight);

        // Decode the input image
        using (var inputData = SKData.Create(inputStream))
            using (var original = SKBitmap.Decode(inputData))
            {
                if (original == null)
                {
                    throw new
                        InvalidOperationException("Failed to decode image from input stream. The stream may not contain a valid image or the format is not supported.");
                }

                // Create new image info with target dimensions
                var imageInfo = new SKImageInfo(scaledWidth, scaledHeight);

                // Use modern SKSamplingOptions with high quality cubic resampler
                var samplingOptions = new SKSamplingOptions(SKCubicResampler.CatmullRom);

                // Resize with high quality sampling
                using (var resized = original.Resize(imageInfo, samplingOptions))
                {
                    if (resized == null)
                    {
                        throw new
                            InvalidOperationException($"Failed to resize image to dimensions {scaledWidth}x{scaledHeight}.");
                    }

                    // Convert to image and encode
                    using (var image = SKImage.FromBitmap(resized))
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            data.SaveTo(memoryStream);
                        }
                }
            }

        // Reset stream position for reading
        memoryStream.Position = 0;

        return memoryStream;
    }
}
