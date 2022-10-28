using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.SimpleUploadService.Extensions;

public class UploadServiceConfigurationBuilder
{
    public IServiceCollection ServiceCollection { get; set; }
    public string             UploadFolder      { get; internal set; } = "Media";
}