using System;

namespace DotNetBrightener.gRPC.Generator.Templates;

internal static class ProtoFileTemplate
{
    internal const string ProtoFileContent = @"
syntax = ""proto3"";

option csharp_namespace = ""{ProtoServiceNamespace}"";

import ""google/api/annotations.proto"";

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

        string methodsCode = "";

        foreach (var method in protoFileDefinition.Methods)
        {
            string methodCode = $@"
    rpc {method.Name} ({method.RequestType?.Name}) returns ({method.ResponseType?.Name}) {{";

            if (method.GenerateRestTranscoding)
            {
                methodCode += $@"
        option (google.api.http) = {{
            {method.RestMethod}: ""{method.RestRouteTemplate}""<__body__>
        }};";

                if (!method.RestMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) && 
                    !method.RestMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    methodCode = methodCode.Replace("<__body__>",
                                                    $@"
            body: ""{method.RestBody}""");
                }
                else
                {
                    methodCode = methodCode.Replace("<__body__>", "");
                }
            }

            methodCode += @"
    }
";
            methodsCode += methodCode;
        }

        generatedCode = generatedCode.Replace(@"{#each Methods}{/each}",
                                              methodsCode);

        string messagesCode = "";

        foreach (var message in protoFileDefinition.Messages)
        {
            string messageCode = $@"
message {message.Name} {{";

            foreach (var field in message.Fields)
            {
                string fieldCode = "";

                if (field.IsRepeated)
                {
                    fieldCode = $@"
    repeated {field.Type} {field.Name} = {field.Index};";
                }
                else
                {
                    fieldCode = $@"
    {field.Type} {field.Name} = {field.Index};";
                }

                messageCode += fieldCode;
            }

            messageCode += @"
}
";
            messagesCode += messageCode;
        }

        generatedCode = generatedCode.Replace("{#each Messages}{/each}",
                                              messagesCode);

        return generatedCode;
    }
}