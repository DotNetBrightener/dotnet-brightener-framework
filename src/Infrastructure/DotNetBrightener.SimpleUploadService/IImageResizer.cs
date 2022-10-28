using System.IO;

namespace DotNetBrightener.SimpleUploadService;

/// <summary>
///     Provides the API for resizing image when uploading
/// </summary>
public interface IImageResizer
{
    /// <summary>
    ///     
    /// </summary>
    /// <param name="inputStream"></param>
    /// <param name="newWidth"></param>
    /// <param name="newHeight"></param>
    /// <returns></returns>
    Stream ResizeImageFromStream(Stream inputStream, int newWidth, int newHeight);
}

internal class NoneImageResizer : IImageResizer
{
    public Stream ResizeImageFromStream(Stream inputStream, int newWidth, int newHeight)
    {
        return inputStream;
    }
}