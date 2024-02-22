using System.Collections.Generic;

namespace DotNetBrightener.gRPC;

public class ProtoFileDefinition
{
    public string PackageName { get; set; }

    public string ServiceName { get; set; }

    public string ProtoServiceNamespace { get; set; }

    public string TargetAssemblyName { get; set; }

    public string ServiceImplClassName { get; set; }

    public string ReferencedServiceName { get; set; }

    public List<ProtoMethodDefinition> Methods { get; set; } = [];

    public List<ProtoMessageDefinition> Messages { get; set; } = [];
}