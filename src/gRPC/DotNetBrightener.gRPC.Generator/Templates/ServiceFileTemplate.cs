using DotNetBrightener.gRPC.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.gRPC.Generator.Templates;

internal class ServiceFileTemplate
{
    internal const string ServiceFileContent = @"
using Grpc.Core;
using {ProtoServiceNamespace};
using DotNetBrightener.gRPC.Extensions;
{MissingUsings}

namespace {AssemblyName}.Services;

public partial class {ServiceImplClassName}
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

    internal const string ServiceFileGContent = @"
{FileHeader}
using Grpc.Core;
using {ProtoServiceNamespace};
using DotNetBrightener.gRPC.Extensions;
{MissingUsings}

namespace {AssemblyName}.Services;

public partial class {ServiceImplClassName} : {ProtoServiceNamespace}.{ServiceName}.{ServiceName}Base
{{#each Methods}{/each}

}";


    public static string Generate(ProtoFileDefinition protoFileDefinition)
    {
        string generatedCode = ServiceFileContent;
        protoFileDefinition.ServiceImplClassName = $"{protoFileDefinition.ServiceName}Impl";

        generatedCode = generatedCode.Replace("{FileHeader}", FileTemplates.GetFileHeader(protoFileDefinition.ServiceImplClassName));
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

        var allCsNamespaces = protoFileDefinition.Messages
                                                 .Select(_ => _.CsNamespace)
                                                 .Distinct()
                                                 .ToList();

        foreach (var method in protoFileDefinition.Methods)
        {
            if (!allCsNamespaces.Contains(method.RequestType.CsNamespace))
            {
                allCsNamespaces.Add(method.RequestType.CsNamespace);
            }

            if (!allCsNamespaces.Contains(method.ResponseType.CsNamespace))
            {
                allCsNamespaces.Add(method.ResponseType.CsNamespace);
            }

            methodsCode.Add($@"
    public override partial async Task<{method.ResponseType?.ProtobufType}> {method.Name}(
            {method.RequestType?.ProtobufType} request, 
            ServerCallContext context)
    {{
        if (context.IsGrpcRequest())
        {{
            // Do something with gRPC request
        }}
        else
        {{
            // Do something with REST request
        }}

        {method.ToCsMethodCall()}

        return result.To{method.ResponseType?.ProtobufType}();
    }}
");
        }

        generatedCode = generatedCode.Replace(@"{#each Methods}{/each}",
                                              string.Join(Environment.NewLine,
                                                          methodsCode));


        generatedCode = generatedCode.Replace("{referencedServiceName}", referencedServiceName);

        var missingUsings = new List<string>();
        allCsNamespaces.ForEach(ns =>
        {
            if (!generatedCode.Contains($"using {ns};"))
            {
                missingUsings.Add($"using {ns};");
            }
        });

        generatedCode = generatedCode.Replace("{MissingUsings}", string.Join(Environment.NewLine, missingUsings));

        return generatedCode;
    }

    public static string GenerateGFile(ProtoFileDefinition protoFileDefinition)
    {
        string generatedCode = ServiceFileGContent;
        protoFileDefinition.ServiceImplClassName = $"{protoFileDefinition.ServiceName}Impl";


        generatedCode = generatedCode.Replace("{FileHeader}", FileTemplates.GetFileHeader(protoFileDefinition.ServiceImplClassName));
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
        var allCsNamespaces = protoFileDefinition.Messages
                                                 .Select(_ => _.CsNamespace)
                                                 .Distinct()
                                                 .ToList();

        foreach (var method in protoFileDefinition.Methods)
        {
            methodsCode.Add($@"
    public override partial Task<{method.ResponseType?.ProtobufType}> {method.Name}(
            {method.RequestType?.ProtobufType} request, 
            ServerCallContext context);
");
        }

        generatedCode = generatedCode.Replace(@"{#each Methods}{/each}",
                                              string.Join(Environment.NewLine,
                                                          methodsCode));


        generatedCode = generatedCode.Replace("{referencedServiceName}", referencedServiceName);

        var missingUsings = new List<string>();
        allCsNamespaces.ForEach(ns =>
        {
            if (!generatedCode.Contains($"using {ns};"))
            {
                missingUsings.Add($"using {ns};");
            }
        });

        generatedCode = generatedCode.Replace("{MissingUsings}", string.Join(Environment.NewLine, missingUsings));

        return generatedCode;
    }
}