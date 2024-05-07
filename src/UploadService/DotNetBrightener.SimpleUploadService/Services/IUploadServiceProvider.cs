using DotNetBrightener.SimpleUploadService.Models;

namespace DotNetBrightener.SimpleUploadService.Services;

public interface IUploadServiceProvider
{
    /// <summary>
    ///     The priority of the provider. The higher number will be higher prioritized.
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Access to the image resizer to perform resizing of images, if needed.
    /// </summary>
    IImageResizer ImageResizer { get; }

    /// <summary>
    ///     Process file upload
    /// </summary>
    /// <param name="fileUploadStream">
    ///     The file stream to upload
    /// </param>
    /// <param name="uploadRequestModel">
    ///     The upload request model
    /// </param>
    /// <param name="fileName">
    ///     The name of upload file
    /// </param>
    /// <param name="baseUrl">
    ///     The current request base url
    /// </param>
    /// <returns></returns>
    Task<FileObjectModel> ProcessUpload(Stream             fileUploadStream,
                                        UploadRequestModel uploadRequestModel,
                                        string             fileName,
                                        string             baseUrl);

    Task<string> GenerateSecuredUrl(string absoluteUrl);
}