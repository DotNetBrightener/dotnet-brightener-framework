﻿using System;
using System.IO;
using System.Net;

namespace DotNetBrightener.gRPC.Generator.Generators;

public partial class ProtoFileGenerator
{
    const string GrpcKeyStartSection = "#region gRPC service registration";

    const string GrpcKeyEndSection = "#endregion gRPC service registration";

    const string GrpcServicePlaceholderKey = "\u2005\u2005\u2005\u2005\u2005\u2005\u2005\u2005\u2005\u2005";

    private string InjectProtobufConfigsIfNeeded(string inputFile)
    {
        var fileContent = File.ReadAllText(inputFile);

        if (!fileContent.Contains(GrpcKeyStartSection))
        {
            fileContent = fileContent.Replace("app.MapGrpcService",
                                              $@"
{GrpcKeyStartSection}
/****************************************************
 -----------------------------------------------------------------------
|          DotNet Brightener gRPC Service Generator Tool                |
|                               ---o0o---                               |
 -----------------------------------------------------------------------

This file is generated by an automation tool and it could be re-generated every time
you build the project.

Don't change this section as your changes will be messed up when the section get re-generated.

© {DateTime.Now.Year} DotNet Brightener. <admin@dotnetbrightener.com>

****************************************************/

{GrpcServicePlaceholderKey}

{GrpcKeyEndSection}

app.MapGrpcService");

        }

        return fileContent;
    }

    private string InjectGrpcService(string fileContent, string grpcServiceName)
    {
        fileContent = fileContent.Replace(GrpcServicePlaceholderKey,
                                          $@"
app.MapGrpcService<{grpcServiceName}>();
{GrpcServicePlaceholderKey}");

        return fileContent;
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
    }
}