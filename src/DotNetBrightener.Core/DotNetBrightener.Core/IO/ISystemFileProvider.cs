using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Core.IO
{
    public interface ISystemFileProvider : IFileProvider
    {
        string SubPath { get; }

        void CreateFolder(string parentPath, string folderName);

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

    public abstract class SystemFileProvider : PhysicalFileProvider, ISystemFileProvider
    {
        protected SystemFileProvider(string root, string subPath) : base(EnsurePathAvailable(root, subPath))
        {
            SubPath = subPath;
        }

        public string SubPath { get; }

        public void CreateFolder(string parentPath, string folderName)
        {
            var normalizedPath = NormalizePath(parentPath);

            folderName = folderName.Trim('/', '\\');

            var fullFolderName = Path.Combine(Root, normalizedPath, folderName);

            if (Directory.Exists(fullFolderName))
                throw new InvalidOperationException($"Folder with name {folderName} has already existed");

            Directory.CreateDirectory(fullFolderName);
        }

        public Task<bool> Exists(string path)
        {
            if (GetFileInfo(path).Exists || GetDirectoryContents(path).Exists)
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public Task<string> ReadFileTextAsync(string fileName)
        {
            var fileInfo = GetFileInfo(fileName);
            if (!fileInfo.Exists)
                throw new InvalidOperationException($"File does not exist");

            return File.ReadAllTextAsync(fileInfo.PhysicalPath);
        }

        public string Combine(string path, string fileName)
        {
            if (string.IsNullOrEmpty(path))
                return NormalizePath(fileName);

            return NormalizePath(Path.Combine(path, fileName));
        }

        public Task<IFileInfo> GetFileInfoAsync(string filePath)
        {
            return Task.FromResult(GetFileInfo(filePath));
        }

        public async Task CreateFileFromStream(string filePath, Stream fileContentStream, bool overwrite = false)
        {
            var physicalPath = GetPhysicalPath(filePath);

            if (!overwrite && File.Exists(physicalPath))
                throw new FileStoreException($"Cannot create file '{filePath}' because it already exists.");

            if (Directory.Exists(physicalPath))
                throw new FileStoreException($"Cannot create file '{filePath}' because it already exists as a directory.");

            // Create directory path if it doesn't exist.
            var physicalDirectoryPath = Path.GetDirectoryName(physicalPath);
            Directory.CreateDirectory(physicalDirectoryPath);
            
            var fileInfo = new FileInfo(physicalPath);

            using (var outputStream = fileInfo.Create())
            {
                await fileContentStream.CopyToAsync(outputStream);
            }
        }

        public Task DeleteItems(string[] itemsToDelete)
        {
            Parallel.ForEach(itemsToDelete,
                             item =>
                             {
                                 var physicalItem = GetFileInfo(item);
                                 if (physicalItem.Exists) // maybe cannot pick because it is folder
                                 {
                                     if (physicalItem.IsDirectory)
                                     {
                                         Directory.Delete(physicalItem.PhysicalPath);
                                     }
                                     else
                                     {
                                         File.Delete(physicalItem.PhysicalPath);
                                     }
                                 }
                                 else
                                 {
                                     if (Directory.Exists(physicalItem.PhysicalPath))
                                     {
                                         Directory.Delete(physicalItem.PhysicalPath);
                                     }
                                 }
                             });

            return Task.CompletedTask;
        }

        public string GetPhysicalPath(string filePath)
        {
            return GetFileInfo(filePath).PhysicalPath;
        }

        public string MapPathFromUrl(string url)
        {
            if (!url.StartsWith("/" + SubPath))
            {
                throw new InvalidOperationException($"Invalid URL");
            }

            return NormalizePath(url)
                  .Trim('/', '\\')
                  .Replace(SubPath, string.Empty);
        }

        public string MapToPublicUrl(string physicalPath)
        {
            var normalizedPath = NormalizePath(physicalPath).Replace(Root, string.Empty);

            return $"/{SubPath}/{NormalizeUrl(normalizedPath)}";
        }

        public string NormalizePath(string path)
        {
            return path.Trim('/', '\\')
                       .Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar);
        }

        public string NormalizeUrl(string path)
        {
            return path.Trim('/', '\\')
                       .Replace('\\', '/');
        }
        
        private static string EnsurePathAvailable(string root, string subPath)
        {
            var combinePath = Path.Combine(root, subPath);

            if (!Directory.Exists(combinePath))
            {
                Directory.CreateDirectory(combinePath);
            }

            return combinePath;
        }
    }
}