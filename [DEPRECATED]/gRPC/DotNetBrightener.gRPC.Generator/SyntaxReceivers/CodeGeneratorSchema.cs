using System.Collections.Generic;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

public class CodeGeneratorSchema
{
    public string GrpcAssemblyPath { get; set; }

    public List<ProtoFileDefinition> ProtoFileDefinitions { get; set; } = new();
}