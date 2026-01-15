using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.SimpleUploadService.Events;
using DotNetBrightener.SimpleUploadService.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace DotNetBrightener.SimpleUploadService.Services;

public interface IUploadService
{
    Task<FileObjectModel> Upload(Stream             fileUploadStream,
                                 UploadRequestModel uploadRequestModel,
                                 string             fileName,
                                 string             currentRequestUrl);
}

public class UploadService : IUploadService
{
    private readonly IEnumerable<IUploadServiceProvider> _uploadServiceProviders;
    private readonly IContentTypeProvider                _contentTypeProvider;
    private readonly IEventPublisher                     _eventPublisher;

    public UploadService(IEnumerable<IUploadServiceProvider> uploadServiceProviders,
                         IEventPublisher                     eventPublisher,
                         IContentTypeProvider                contentTypeProvider)
    {
        _eventPublisher         = eventPublisher;
        _contentTypeProvider    = contentTypeProvider;
        _uploadServiceProviders = uploadServiceProviders.OrderByDescending(p => p.Priority);
    }

    public async Task<FileObjectModel> Upload(Stream             fileUploadStream,
                                              UploadRequestModel uploadRequestModel,
                                              string             fileName,
                                              string             currentRequestUrl)
    {
        _contentTypeProvider.TryGetContentType(fileName, out var contentType);
        uploadRequestModel.ContentType = contentType;

        foreach (var uploadServiceProvider in _uploadServiceProviders)
        {
            var uploadResult =
                await uploadServiceProvider.ProcessUpload(fileUploadStream,
                                                          uploadRequestModel,
                                                          fileName,
                                                          currentRequestUrl);

            if (uploadResult != null)
            {
                var eventMessage = new FileUploadedEventMessage
                {
                    UploadedFileInfo = uploadResult,
                    UploadRequest    = uploadRequestModel
                };

                await _eventPublisher.Publish(eventMessage);

                return eventMessage.ResultUploadedFileInfo ?? uploadResult;
            }
        }

        return null;
    }
}