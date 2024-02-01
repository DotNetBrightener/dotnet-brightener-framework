namespace DotNetBrightener.gRPC;

internal class ProtoMessageFieldDefinition
{
    public string Name { get; set; }

    public string Type { get; set; }

    public int Index { get; set; }

    public bool IsRepeated { get; set; }
}