using System;
using System.Text;

namespace DotNetBrightener.gRPC.Generator.Templates;

internal static class ProtoFileTemplate
{
    internal const string ProtoFileContent = @"
syntax = ""proto3"";

option csharp_namespace = ""{ProtoServiceNamespace}"";

import ""google/api/annotations.proto"";
import ""google/protobuf/timestamp.proto"";

package {PackageName};

service {ServiceName} {
    {#each Methods}{/each}
}

{#each Messages}{/each}
";

    public static string Generate(ProtoFileDefinition protoFileDefinition)
    {
        string generatedCode = ProtoFileContent;

        generatedCode = generatedCode.Replace("{ProtoServiceNamespace}", protoFileDefinition.ProtoServiceNamespace);
        generatedCode = generatedCode.Replace("{PackageName}", protoFileDefinition.PackageName);
        generatedCode = generatedCode.Replace("{ServiceName}", protoFileDefinition.ServiceName);

        var methodsCodeBuilder = new StringBuilder();

        foreach (var method in protoFileDefinition.Methods)
        {
            methodsCodeBuilder.AppendLine();
            methodsCodeBuilder.AppendLine(method.ToProtoDefinition());
        }

        generatedCode = generatedCode.Replace(@"{#each Methods}{/each}",
                                              methodsCodeBuilder.ToString());

        var messagesCodeBuilder = new StringBuilder();

        foreach (var message in protoFileDefinition.Messages)
        {
            messagesCodeBuilder.AppendLine(message.ToString());
        }

        generatedCode = generatedCode.Replace("{#each Messages}{/each}",
                                              messagesCodeBuilder.ToString());

        return generatedCode;
    }
}