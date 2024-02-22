namespace DotNetBrightener.gRPC;

public class ProtoMessageFieldDefinition
{
    public string Name { get; set; }

    public string ProtobufType { get; set; }

    public string CsType { get; set; }

    public int Index { get; set; }

    public bool IsRepeated { get; set; }

    public bool IsOptional { get; set; }

    public override string ToString()
    {
        return $"{(IsOptional ? "optional " : "")}{(IsRepeated ? "repeated " : "")}{ProtobufType} {Name.ToSnakeCase()} = {Index};";
    }
}