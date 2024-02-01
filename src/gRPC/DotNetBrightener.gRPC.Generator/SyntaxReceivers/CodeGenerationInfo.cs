using System.Collections.Generic;
using DotNetBrightener.gRPC.Generator.Templates;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

internal class CodeGenerationInfo
{
    public string GrpcAssemblyPath { get; set; }
    public string GrpcNamespace    { get; set; }
    public string ProtoFilePath    { get; set; }
    public string ServiceFilePath  { get; set; }

    public List<ProtoFileDefinition> ProtoFileDefinitions { get; set; } = new();
}