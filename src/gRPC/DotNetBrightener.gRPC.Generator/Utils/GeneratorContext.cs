using System.Collections.Generic;
using DotNetBrightener.gRPC.Generator.SyntaxReceivers;

namespace DotNetBrightener.gRPC.Generator.Utils;

internal static class GeneratorContext
{
    internal static readonly List<ListTypeMapping> AvailableListTypes = new()
    {
        new ListTypeMapping
        {
            ConstructedFromString = "System.Collections.Generic.List<T>",
            Type                  = typeof(List<>),
            AlternativeType       = typeof(List<>)
        },
        new ListTypeMapping
        {
            ConstructedFromString = "System.Collections.Generic.IEnumerable<T>",
            Type                  = typeof(IEnumerable<>),
            AlternativeType       = typeof(List<>)
        },
        new ListTypeMapping
        {
            ConstructedFromString = "System.Collections.Generic.ICollection<T>",
            Type                  = typeof(ICollection<>),
            AlternativeType       = typeof(List<>)
        },
    };
}