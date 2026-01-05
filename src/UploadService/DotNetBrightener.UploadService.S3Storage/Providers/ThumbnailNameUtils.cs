namespace DotNetBrightener.UploadService.S3Storage.Providers;

public static class ThumbnailNameUtils
{
    public static string GetThumbnailFileName(string originalFileName,
                                              int    thumbWidth  = 0,
                                              int    thumbHeight = 0)
    {
        var blobNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        string thumbnailFileName;

        if (thumbHeight == 0)
        {
            if (thumbWidth == 0)
            {
                return originalFileName;
            }

            thumbnailFileName = blobNameWithoutExtension + $"w_{thumbWidth}";
        }
        else
        {
            var thumbName = thumbWidth == 0
                                ? $"h_{thumbHeight}"
                                : $"w_{thumbWidth}_h_{thumbHeight}";

            thumbnailFileName = blobNameWithoutExtension + thumbName;
        }


        thumbnailFileName = $"{thumbnailFileName}{Path.GetExtension(originalFileName)}";

        return thumbnailFileName;
    }
}