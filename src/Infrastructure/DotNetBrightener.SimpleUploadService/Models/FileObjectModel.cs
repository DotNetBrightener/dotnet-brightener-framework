using Newtonsoft.Json;

namespace DotNetBrightener.SimpleUploadService.Models;

public class FileObjectModel
{
    public string Name { get; set; }
    public long   Size { get; set; }

    [JsonIgnore]
    public string Folder { get; set; }

    public string AbsoluteUrl { get; set; }
    public string RelativeUrl { get; set; }
    public string Mime        { get; set; }
    public string Error       { get; set; }
}