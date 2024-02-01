using System.Collections.Generic;

namespace DotNetBrightener.gRPC;

internal class ProtoMessageDefinition
{
    private List<ProtoMessageFieldDefinition> _fields = [];

    public string Name { get; set; }

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
}