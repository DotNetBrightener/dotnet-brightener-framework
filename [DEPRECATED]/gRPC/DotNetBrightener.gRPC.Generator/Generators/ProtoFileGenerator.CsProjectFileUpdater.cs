using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace DotNetBrightener.gRPC.Generator.Generators;

public partial class ProtoFileGenerator
{
    private const string ProjFileProtobufIndicator = $@"
            <!-- 
                DotNet Brightener gRPC Generated Protobuf included here. DO NOT remove this comment!
            -->";

    private void UpdateCsprojFile(string projectPath, List<string> protoFilesList)
    {
        var csprojFile = Directory.GetFiles(projectPath, "*.csproj").First();

        var csprojFileContent = File.ReadAllText(csprojFile);
        

        var linesToInsert = new List<string>();

        foreach (var className in protoFilesList.Distinct())
        {
            var protobufItem = $"<Protobuf Include=\"Protos\\{className}.proto\" GrpcServices=\"Server\" />";

            if (!csprojFileContent.Contains(protobufItem))
            {
                linesToInsert.Add($"\t\t{protobufItem}");
            }
        }

        // check if any protobuf file already exists in the csproj file
        var existingProtobuf = csprojFileContent.Contains(ProjFileProtobufIndicator);

        if (!existingProtobuf)
        {
            linesToInsert.Insert(0, "\t<ItemGroup>");
            linesToInsert.Add(ProjFileProtobufIndicator);
            linesToInsert.Add("\t</ItemGroup>");
            linesToInsert.Add("</Project>");

            csprojFileContent = csprojFileContent.Replace("</Project>",
                                                          string.Join(Environment.NewLine, linesToInsert));
        }
        else
        {
            linesToInsert.Add(ProjFileProtobufIndicator);
            csprojFileContent = csprojFileContent.Replace(ProjFileProtobufIndicator,
                                                          string.Join(Environment.NewLine, linesToInsert));
        }

        File.WriteAllText(csprojFile, csprojFileContent);
    }

    private void AddAnnotationFilesIfNeeded(string controllerAssemblyPath)
    {
        var googleApiFolderPath = Path.Combine(controllerAssemblyPath, "google", "api");

        if (!Directory.Exists(googleApiFolderPath))
        {
            Directory.CreateDirectory(googleApiFolderPath);
        }

        var httpProtoFilePath = Path.Combine(googleApiFolderPath, "http.proto");

        if (!File.Exists(httpProtoFilePath))
        {
            using var client = new WebClient();

            client.DownloadFile("https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/Grpc/JsonTranscoding/test/testassets/Sandbox/google/api/http.proto",
                                httpProtoFilePath);
        }

        var annotationFilePath = Path.Combine(googleApiFolderPath, "annotations.proto");

        if (!File.Exists(annotationFilePath))
        {
            using var client = new WebClient();

            client.DownloadFile("https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/Grpc/JsonTranscoding/test/testassets/Sandbox/google/api/annotations.proto",
                                annotationFilePath);
        }

        var googleProtobufFolderPath    = Path.Combine(controllerAssemblyPath, "google", "protobuf");
        var timestampProtoFilePath = Path.Combine(googleProtobufFolderPath, "timestamp.proto");

        if (!File.Exists(timestampProtoFilePath))
        {
            using var client = new WebClient();

            client.DownloadFile("https://raw.githubusercontent.com/protocolbuffers/protobuf/main/src/google/protobuf/timestamp.proto",
                                timestampProtoFilePath);
        }
    }
}