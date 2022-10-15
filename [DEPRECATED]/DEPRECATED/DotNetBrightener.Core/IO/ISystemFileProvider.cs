using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.IO;

/// <summary>
///     A customized of <see cref="IFileProvider"/> that provides more functionalities to manupilate with file system
/// </summary>
public interface ISystemFileProvider : IFileProvider
{
    /// <summary>
    ///     The folder that is treated as root of this <see cref="ISystemFileProvider"/>
    /// </summary>
    string SubPath { get; }

    /// <summary>
    ///     Creates a folder into a specified folder
    /// </summary>
    /// <param name="parentPath">
    ///     The path to the parent folder, which is sub folder of the <see cref="SubPath"/>
    /// </param>
    /// <param name="folderName"></param>
    void CreateFolder(string parentPath, string folderName);

    /// <summary>
    ///     Checks if the given path exists under the root of the <see cref="ISystemFileProvider"/>
    /// </summary>
    /// <param name="path">
    ///     The relative path to check
    /// </param>
    /// <returns>
    ///     <c>true</c> if the path exists, both as folder or file. Otherwise, <c>false</c>
    /// </returns>
    Task<bool> Exists(string path);

    Task<string> ReadFileTextAsync(string fileName);

    string Combine(string path, string fileName);
        
    Task<IFileInfo> GetFileInfoAsync(string filePath);

    Task CreateFileFromStream(string filePath, Stream fileContentStream, bool overwrite = false);
        
    Task DeleteItems(string[] itemsToDelete);

    string MapToPublicUrl(string physicalPath);

    string GetPhysicalPath(string filePath);

    string MapPathFromUrl(string url);

    string NormalizePath(string path);
}