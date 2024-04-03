namespace DotNetBrightener.SimpleUploadService.Models;

public class UploadRequestModel
{
    /// <summary>
    ///     The path to store the uploaded file
    /// </summary>
    public string Path { get; set; }

    public List<ThumbnailGenerateRequestModel> ThumbnailGenerateRequests { get; set; }
}