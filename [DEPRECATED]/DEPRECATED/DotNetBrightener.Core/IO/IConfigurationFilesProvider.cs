using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.IO;

/// <summary>
///     An extension for <see cref="IFileProvider"/> service which provides abstract access to the files which contain the configurations
/// </summary>
public interface IConfigurationFilesProvider : IFileProvider
{

}