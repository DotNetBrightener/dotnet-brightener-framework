using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.IO;

internal class DefaultConfigurationFilesProvider : PhysicalFileProvider, IConfigurationFilesProvider
{
    internal const string ConfigurationFolder = "config";

    private DefaultConfigurationFilesProvider(string root) : base(root)
    {

    }

    internal static DefaultConfigurationFilesProvider Init(IWebHostEnvironment webHostEnvironment)
    {
        var rootPath = Path.Combine(webHostEnvironment.ContentRootPath, ConfigurationFolder);
        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

        return new DefaultConfigurationFilesProvider(rootPath);
    }
}