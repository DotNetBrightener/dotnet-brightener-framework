using DotNetBrightener.gRPC.Generator.Templates;
using System.IO;
using DotNetBrightener.gRPC.Generator.Models;

namespace DotNetBrightener.gRPC.Generator.Generators;

public partial class ProtoFileGenerator
{
    private static ProtoFileModel GenerateProtoFile(string targetFolder, ProtoFileDefinition modelClass)
    {
        var className = $"{modelClass.ServiceName}";

        var defaultPathFile = Path.Combine(targetFolder, $"{className}.proto");

        var fileContent = ProtoFileTemplate.Generate(modelClass);

        return new ProtoFileModel
        {
            ClassName = className,
            FilePath = defaultPathFile,
            FileName = $"{className}.proto",
            FileContent = fileContent
        };
    }
}