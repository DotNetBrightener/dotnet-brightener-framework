using DotNetBrightener.Core.Localization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.FileProviders;

public static class FileInfoExtensions
{
    public static TranslationFileInfo ToTranslationFileInfo(this IFileInfo fileInfo)
    {
        return new TranslationFileInfo(new System.IO.FileInfo(fileInfo.PhysicalPath));
    }
}