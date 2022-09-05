using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Core.Modular.Extensions;

public static class TypeMetadataCollectionExtensions
{
    public static ITypeMetadata FindMetadata(this IEnumerable<ITypeMetadata> collection, Type lookUpType)
    {
        return collection.FirstOrDefault(_ => _.Type == lookUpType);
    }
}