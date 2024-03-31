using System.Collections.Generic;
using System.Text;

namespace DotNetBrightener.gRPC;

public class ProtoMessageDefinition
{
    internal bool IsSingleType { get; set; }

    private List<ProtoMessageFieldDefinition> _fields = [];

    public string ProtoServiceNamespace { get; set; }
    public string ProtobufType      { get; set; }
    public string CsType            { get; set; }
    public string CsNamespace           { get; set; }

    public List<ProtoMessageFieldDefinition> Fields
    {
        get => _fields;
        set
        {
            _fields = value;

            foreach (var field in _fields)
            {
                field.Index = _fields.IndexOf(field) + 1;
            }
        }
    }

    public override string ToString()
    {
        var messagesCodeBuilder = new StringBuilder();
        messagesCodeBuilder.AppendLine();
        messagesCodeBuilder.AppendLine($"message {ProtobufType} {{");

        foreach (var field in Fields)
        {
            messagesCodeBuilder.AppendLine($"   {field}");
        }

        messagesCodeBuilder.AppendLine("}");

        return messagesCodeBuilder.ToString();
    }
}