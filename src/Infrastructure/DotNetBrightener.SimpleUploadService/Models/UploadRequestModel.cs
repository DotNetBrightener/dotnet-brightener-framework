namespace DotNetBrightener.SimpleUploadService.Models;

public class UploadRequestModel
{
    /// <summary>
    ///     The path to store the uploaded file
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///      Set content type of the file
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    ///     Specifies that the upload should happen in the background
    /// </summary>
    public bool UploadInBackground { get; set; }

    /// <summary>
    ///     Specifies that the upload should be done for the thumbnails only
    /// </summary>
    public bool OnlyUploadThumbnails { get; set; }

    /// <summary>
    ///     Indicates that the upload is for the thumbnail
    /// </summary>
    public bool IsThumbnailUpload { get; set; }

    public List<ThumbnailGenerateRequestModel> ThumbnailGenerateRequests { get; set; } = [];
}