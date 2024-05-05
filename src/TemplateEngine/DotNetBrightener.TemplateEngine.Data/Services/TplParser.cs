using DotNetBrightener.TemplateEngine.Data.Models;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.TemplateEngine.Data.Services;

internal static class TplParser
{
    private const string TitleSectionStart = "## TITLE BEGIN ##";
    private const string TitleSectionEnd   = "## TITLE END ##";

    public static void WriteTemplate(TemplateModelDto content, IFileInfo templateFile)
    {
        var fileContent = $@"
{TitleSectionStart}
{content.TemplateTitle}
{TitleSectionEnd}

{content.TemplateContent}
";

        File.WriteAllText(templateFile.PhysicalPath!, fileContent.Trim());
    }

    public static TemplateModelDto ReadTemplate(IFileInfo templateFile)
    {
        if (!templateFile.Exists)
            return default;

        var fileContent = File.ReadAllText(templateFile.PhysicalPath!);

        var templateContent = GetInnerSection(fileContent, TitleSectionEnd);

        return new TemplateModelDto
        {
            TemplateType = Path.GetFileNameWithoutExtension(templateFile.PhysicalPath),
            TemplateContent = templateContent,
            TemplateTitle = GetInnerSection(fileContent, TitleSectionStart, TitleSectionEnd),
        };
    }

    private static string GetInnerSection(string fileContent,
                                          string titleSectionStart,
                                          string titleSectionEnd = null)
    {
        var startIndex = fileContent.IndexOf(titleSectionStart, StringComparison.Ordinal);
        var endIndex = !string.IsNullOrEmpty(titleSectionEnd)
                           ? fileContent.IndexOf(titleSectionEnd, StringComparison.Ordinal)
                           : fileContent.Length;

        if (startIndex == -1 ||
            endIndex == -1)
            return string.Empty;

        return fileContent.Substring(startIndex + titleSectionStart.Length,
                                     endIndex - startIndex - titleSectionStart.Length)
                          .Trim();
    }
}