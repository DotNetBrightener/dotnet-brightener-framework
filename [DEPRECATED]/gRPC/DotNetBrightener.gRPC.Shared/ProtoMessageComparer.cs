using System.Collections.Generic;

namespace DotNetBrightener.gRPC;

internal class ProtoMessageComparer : IEqualityComparer<ProtoMessageDefinition>
{
    public bool Equals(ProtoMessageDefinition x, ProtoMessageDefinition y)
    {
        return x.ProtobufType == y.ProtobufType;
    }

    public int GetHashCode(ProtoMessageDefinition obj)
    {
        return obj.ProtobufType.GetHashCode();
    }
}