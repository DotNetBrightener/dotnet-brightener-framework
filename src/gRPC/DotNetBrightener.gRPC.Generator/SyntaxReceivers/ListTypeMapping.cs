using System;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

internal class ListTypeMapping
{
    public string ConstructedFromString { get; set; }

    public Type Type { get; set; }

    public Type AlternativeType { get; set; }
}