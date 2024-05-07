using DotNetBrightener.SimpleUploadService;
using ImageMagick;

namespace DotNetBrightener.UploadService.ImageOptimizer;

public class ImageMagickOptimizer : IImageResizer
{
    public Stream ResizeImageFromStream(Stream inputStream, int newWidth, int newHeight)
    {
        var memoryStream = new MemoryStream();

        using (var image = new MagickImage(inputStream))
        {
            var newSize = new MagickGeometry(newWidth, newHeight);

            image.Resize(newSize);

            image.Write(memoryStream);
        }

        return memoryStream;
    }
}