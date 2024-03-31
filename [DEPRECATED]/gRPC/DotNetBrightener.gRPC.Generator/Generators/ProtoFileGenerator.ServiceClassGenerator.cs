using DotNetBrightener.gRPC.Generator.Templates;
using System.IO;
using DotNetBrightener.gRPC.Generator.Models;

namespace DotNetBrightener.gRPC.Generator.Generators;

public partial class ProtoFileGenerator
{
    private static ServiceImplFileModel GenerateServiceFile(string targetFolder, ProtoFileDefinition modelClass)
    {
        var className = modelClass.ServiceImplClassName;

        var fileContent = ServiceFileTemplate.Generate(modelClass);

        var defaultPathFile = Path.Combine(targetFolder, $"{className}.cs");

        return new ServiceImplFileModel
        {
            ClassName   = className,
            FilePath    = defaultPathFile,
            FileName    = $"{className}.cs",
            FileContent = fileContent
        };
    }

    private static ServiceFileModel GenerateGServiceFile(string targetFolder, ProtoFileDefinition modelClass)
    {
        var gfileContent = ServiceFileTemplate.GenerateGFile(modelClass);
        var className = modelClass.ServiceImplClassName;

        return new ServiceFileModel
        {

            ClassName   = className,
            FilePath    = Path.Combine(targetFolder, $"{className}.g.cs"),
            FileName    = $"{className}.g.cs",
            FileContent = gfileContent
        };
    }

    private static MessageFileModel GenerateMessageConvertorFile(string targetFolder, ProtoMessageDefinition modelClass)
    {
        if (modelClass.IsSingleType) 
            return null ;

        var fileContent = ClassTypeTemplate.Generate(modelClass);

        var className = modelClass.ProtobufType;

        var defaultPathFile = Path.Combine(targetFolder, $"{className}.g.cs");

        return new MessageFileModel
        {
            ClassName   = className,
            FilePath    = defaultPathFile,
            FileName    = $"{className}.g.cs",
            FileContent = fileContent
        };
    }
}