using System;
using System.Collections.Generic;

namespace DotNetBrightener.gRPC.Generator.Templates;

internal class ServiceFileTemplate
{
    internal const string ServiceFileContent = @"
using Grpc.Core;
using {ProtoServiceNamespace};

namespace {AssemblyName}.Services;

public partial class {ServiceImplClassName} : {ProtoServiceNamespace}.{ServiceName}.{ServiceName}Base
{
    private readonly ILogger _logger;
    private readonly {ReferencedServiceName} _{referencedServiceName};

    public {ServiceImplClassName}(
        ILogger<{ServiceImplClassName}> logger,
        {ReferencedServiceName} {referencedServiceName})
    {
        _logger = logger;
        _{referencedServiceName} = {referencedServiceName};
    }

    {#each Methods}{/each}

}";


    public static string Generate(ProtoFileDefinition protoFileDefinition)
    {
        string generatedCode = ServiceFileContent;
        protoFileDefinition.ServiceImplClassName = $"{protoFileDefinition.ServiceName}Impl";

        generatedCode = generatedCode.Replace("{ProtoServiceNamespace}", protoFileDefinition.ProtoServiceNamespace);
        generatedCode = generatedCode.Replace("{PackageName}", protoFileDefinition.PackageName);
        generatedCode = generatedCode.Replace("{ServiceName}", protoFileDefinition.ServiceName);
        generatedCode = generatedCode.Replace("{AssemblyName}", protoFileDefinition.TargetAssemblyName);
        generatedCode = generatedCode.Replace("{ServiceImplClassName}", protoFileDefinition.ServiceImplClassName);
        generatedCode = generatedCode.Replace("{ReferencedServiceName}", protoFileDefinition.ReferencedServiceName);

        var referencedServiceName = protoFileDefinition.ReferencedServiceName
                                                       .TrimStart('I');

        referencedServiceName = char.ToLower(referencedServiceName[0]) + referencedServiceName.Substring(1);

        var methodsCode = new List<string>();

        foreach (var method in protoFileDefinition.Methods)
        {
            methodsCode.Add($@"
    public override async Task<{method.ResponseType?.Name}> {method.Name}(
            {method.RequestType?.Name} request, 
            ServerCallContext context)
    {{
        // TODO: Implement {method.Name}

        // var result = await _{referencedServiceName}.{method.Name}(request);

        return new {method.ResponseType?.Name}
        {{

        }};
    }}
");
        }

        generatedCode = generatedCode.Replace(@"{#each Methods}{/each}",
                                              string.Join(Environment.NewLine,
                                                          methodsCode));


        generatedCode = generatedCode.Replace("{referencedServiceName}", referencedServiceName);



        return generatedCode;
    }
}