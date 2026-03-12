namespace WebApi.GenericCRUD.Generator.Utils;

internal static class FileTemplates
{
    public static string GetFileHeader(string className)
    {
        return $@"
/****************************************************

 -----------------------------------------------------------------------
|            Vampire Coder Auto CRUD Web API Generator Tool             |
|                               ---o0o---                               |
 -----------------------------------------------------------------------

               ⚠️  WARNING: DO NOT MODIFY THIS FILE! ⚠️

This file contains auto-generated core logic that is embedded in the compilation context.
Any changes made to this file will be lost when the project is rebuilt.

If you need to customize the behavior for this entity, use the corresponding {className}.cs
partial class file which allows you to extend and override the generated functionality.

© {DateTime.Now.Year} Vampire Coder (Formerly DotNet Brightener). <admin@vampirecoder.com>

****************************************************/";
    }
}

internal static class FilePathHelper
{
    internal static string GetAssemblyPath(this string lookingPath)
    {
        var expectingAssemblyDirectory = Path.GetDirectoryName(lookingPath);

        while (expectingAssemblyDirectory != null)
        {
            var files = Directory.GetFiles(expectingAssemblyDirectory, "*.csproj");

            if (files.Count() == 1)
            {
                return expectingAssemblyDirectory;
            }

            expectingAssemblyDirectory = Directory.GetParent(expectingAssemblyDirectory)?.FullName;
        }

        return null;
    }
}
