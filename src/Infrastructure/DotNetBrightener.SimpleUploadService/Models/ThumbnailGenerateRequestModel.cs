using System.IO;

namespace DotNetBrightener.SimpleUploadService.Models;

public class ThumbnailGenerateRequestModel
{
    public int ThumbnailWidth { get; set; }

    public int ThumbnailHeight { get; set; }

    public Stream GeneratedThumbnailStream { get; set; }
}