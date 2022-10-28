using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.SimpleUploadService.Models;

namespace DotNetBrightener.SimpleUploadService.Events;

public class FileUploadedEventMessage: IEventMessage
{
    public UploadRequestModel UploadRequest { get; set; }

    public FileObjectModel UploadedFileInfo { get; set; }

    public FileObjectModel ResultUploadedFileInfo { get; set; }
}