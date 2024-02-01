using DotNetBrightener.gRPC.Generator.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetBrightener.gRPC.Generator.Generators;

[Generator]
public partial class ProtoFileGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AutoGenerateProtoFileReceiver());
    }

    /// <summary>
    /// And consume the receiver here.
    /// </summary>
    public void Execute(GeneratorExecutionContext context)
    {
        var models = (context.SyntaxContextReceiver as AutoGenerateProtoFileReceiver).Models;

        if (models is null)
            return;

        var controllerAssemblyPath = models.GrpcAssemblyPath;

        var programCsFile = Path.Combine(controllerAssemblyPath, "Program.cs");

        string startupCsFileContent = string.Empty;

        if (File.Exists(programCsFile))
        {
            startupCsFileContent = InjectProtobufConfigsIfNeeded(programCsFile);
        }

        AddAnnotationFilesIfNeeded(controllerAssemblyPath);

        var protoFilesList = new List<string>();

        foreach (var modelClass in models.ProtoFileDefinitions)
        {
            var protoClassName = GenerateProtoFile(Path.Combine(controllerAssemblyPath, "Protos"), modelClass);
            protoFilesList.Add(protoClassName);
            var serviceImplClassName =
                GenerateServiceFile(Path.Combine(controllerAssemblyPath, "Services"), modelClass);

            if (serviceImplClassName is not null)
            {
                startupCsFileContent = InjectGrpcService(startupCsFileContent, serviceImplClassName);
            }
        }
        
        File.WriteAllText(programCsFile, startupCsFileContent);

        UpdateCsprojFile(controllerAssemblyPath, protoFilesList);
    }

    private void UpdateCsprojFile(string projectPath, List<string> protoFilesList)
    {
        var csprojFile = Directory.GetFiles(projectPath, "*.csproj").First();

        var csprojFileContent = File.ReadAllText(csprojFile);

        // look for the last GRPC service in the file
        const string grpcServerKeyword = " GrpcServices=\"Server\" />";

        foreach (var className in protoFilesList)
        {
            var lastGrpcPosition =
                csprojFileContent.LastIndexOf(grpcServerKeyword, StringComparison.OrdinalIgnoreCase) +
                grpcServerKeyword.Length;

            if (!csprojFileContent
                   .Contains($"<Protobuf Include=\"Protos\\{className}.proto\"{grpcServerKeyword}"))
            {
                var insertNewGrpcProto = $"{Environment.NewLine}" +
                                         $"\t\t<Protobuf Include=\"Protos\\{className}.proto\"{grpcServerKeyword}";
                csprojFileContent = csprojFileContent.Insert(lastGrpcPosition, insertNewGrpcProto);
            }
        }

        File.WriteAllText(csprojFile, csprojFileContent);
    }
}