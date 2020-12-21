using System;
using System.IO;
using System.Linq;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine
{
    public static class FilePathExtensions
    {
        public static string GetRelativePath(this string initialFilePath, string relativeFilePath)
        {
            if (initialFilePath == null)
                return string.Empty;

            var normalizedPath = initialFilePath.NormalizeFilePath();

            var folderPath = Directory.Exists(normalizedPath)
                                 ? normalizedPath
                                 : Path.GetDirectoryName(normalizedPath);

            var normalizedRelativeFile = relativeFilePath.NormalizeFilePath();

            // see how many upper folders defined
            var numberOfParentsFolder =
                normalizedRelativeFile.Split(new[] { $"..{Path.DirectorySeparatorChar}" }, StringSplitOptions.None)
                                      .Count(_ => _ == string.Empty);

            while (numberOfParentsFolder > 0)
            {
                folderPath = Path.GetDirectoryName(folderPath); // go up 1 level from previous folder
                numberOfParentsFolder--;
            }

            return Path.Combine(folderPath,
                                normalizedRelativeFile.Split(new[] { $"..{Path.DirectorySeparatorChar}" },
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                      .FirstOrDefault());
        }

        public static string CombinePath(this string filePath, string path2)
        {
            return Path.Combine(filePath, path2);
        }

        public static string NormalizeFilePath(this string filePath)
        {
            return filePath.Replace("\\", Path.DirectorySeparatorChar.ToString())
                           .Replace("/", Path.DirectorySeparatorChar.ToString())
                           .Trim('/', '\\');
        }

        public static string ToUrl(this string filePath)
        {
            return filePath.Replace("\\", "/");
        }
    }
}