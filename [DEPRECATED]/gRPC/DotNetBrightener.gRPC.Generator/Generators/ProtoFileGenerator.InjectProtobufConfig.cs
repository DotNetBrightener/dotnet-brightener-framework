﻿using System;
using System.IO;

namespace DotNetBrightener.gRPC.Generator.Generators;

public partial class ProtoFileGenerator
{
    const string GrpcKeyStartSection = "#region gRPC service auto-registration";

    const string GrpcKeyEndSection = "#endregion gRPC service auto-registration";

    const string GrpcServicePlaceholderKey = "/**\u2005\u2005 Auto registration of gRPC services will be here. DO NOT remove this comment \u2005\u2005**/";

    private string InjectProtobufConfigsIfNeeded(string inputFile)
    {
        var fileContent = File.ReadAllText(inputFile);

        if (!fileContent.Contains(GrpcKeyStartSection))
        {
            // Add JsonTranscoding to the gRPC service
            fileContent = fileContent.Replace("builder.Services.AddGrpc();", 
                                              @"// Including JsonTranscoding for RestAPI support. Added by DotNet Brightener gRPC Generator
builder.Services.AddGrpc().AddJsonTranscoding();");

            // register gRPC-Web
            fileContent = fileContent.Replace("var app = builder.Build();",
                                              $@"var app = builder.Build();

// Including Routing for RestAPI support. Added by DotNet Brightener gRPC Generator
app.UseRouting();

// Including Grpc Web Support. Added by DotNet Brightener gRPC Generator
app.UseGrpcWeb(new GrpcWebOptions {{ DefaultEnabled = true }});

{GrpcKeyStartSection}
/****************************************************
 -----------------------------------------------------------------------
|          DotNet Brightener gRPC Service Generator Tool                |
|                               ---o0o---                               |
 -----------------------------------------------------------------------

This section of the file is generated by an automation tool, and it could 
be re-generated every time you build the project.

Don't change this section as your changes will be messed up if the section gets re-generated.

© {DateTime.Now.Year} DotNet Brightener. <admin@dotnetbrightener.com>

****************************************************/

{GrpcServicePlaceholderKey}
{GrpcKeyEndSection}

");

        }

        return fileContent;
    }

    private string InjectGrpcService(string fileContent, string grpcServiceName)
    {
        var newServiceLine = $"app.MapGrpcService<{grpcServiceName}>();";

        if (fileContent.Contains(newServiceLine))
        {
            return fileContent;
        }

        fileContent = fileContent.Replace(GrpcServicePlaceholderKey,
                                          $@"
{newServiceLine}
{GrpcServicePlaceholderKey}");

        return fileContent;
    }
}