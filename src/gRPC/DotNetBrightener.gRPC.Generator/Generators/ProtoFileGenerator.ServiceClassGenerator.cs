using DotNetBrightener.gRPC.Generator.Templates;
using System.IO;

namespace DotNetBrightener.gRPC.Generator.Generators;

public partial class ProtoFileGenerator
{
    private static string GenerateServiceFile(string targetFolder, ProtoFileDefinition modelClass)
    {
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var fileContent = ServiceFileTemplate.Generate(modelClass);

        var className = modelClass.ServiceImplClassName;

        var defaultPathFile = Path.Combine(targetFolder, $"{className}.cs");

        if (!File.Exists(defaultPathFile))
        {
            File.WriteAllText(defaultPathFile, fileContent);

            return className;
        }

        return null;
    }
}