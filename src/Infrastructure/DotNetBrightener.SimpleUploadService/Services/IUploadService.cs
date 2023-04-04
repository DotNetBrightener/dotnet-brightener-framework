using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.SimpleUploadService.Events;
using DotNetBrightener.SimpleUploadService.Models;

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
    private readonly IEventPublisher                     _eventPublisher;

    public UploadService(IEnumerable<IUploadServiceProvider> uploadServiceProviders,
                         IEventPublisher                     eventPublisher)
    {
        _eventPublisher         = eventPublisher;
        _uploadServiceProviders = uploadServiceProviders.OrderByDescending(_ => _.Priority);
    }

    public async Task<FileObjectModel> Upload(Stream             fileUploadStream,
                                              UploadRequestModel uploadRequestModel,
                                              string             fileName,
                                              string             currentRequestUrl)
    {
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