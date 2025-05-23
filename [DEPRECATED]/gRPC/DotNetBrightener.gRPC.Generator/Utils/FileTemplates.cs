﻿using System;
using System.IO;
using System.Linq;

namespace DotNetBrightener.gRPC.Generator.Utils;

internal static class FileTemplates
{
    public static string GetFileHeader(string className)
    {
        return $@"/****************************************************

 -----------------------------------------------------------------------------
|                DotNet Brightener gRPC Service Generator Tool                |
|                                  ---o0o---                                  |
 -----------------------------------------------------------------------------

This file is generated by an automation tool and is re-generated every time
you build the project.

Don't change this file as your changes will be lost when the file is re-generated.
If you need to change the code for this class, update the {className}.cs file
which should be in the same folder as this file.

© {DateTime.Now.Year} DotNet Brightener. <admin@dotnetbrightener.com>

****************************************************/";
    }
}

internal static class FilePathHelper
{
    internal static string GetAssemblyPath(this string lookingPath)
    {
        var expectingAssemblyDirectory = Path.GetDirectoryName(lookingPath);

        while (true)
        {
            var files = Directory.GetFiles(expectingAssemblyDirectory, "*.csproj")
                                 .Where(filePath => !Path.GetFileNameWithoutExtension(filePath).Contains("- Backup"));

            if (files.Count() == 1)
            {
                return expectingAssemblyDirectory;
            }

            expectingAssemblyDirectory = Directory.GetParent(expectingAssemblyDirectory)?.FullName;
        }

        return null;
    }
}