using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Core.Utils;

public static class TypeArrayExtensions
{
    public static IEnumerable<Type> TypesDerivedFrom<TType>(this IEnumerable<Type> typesList)
    {
        return typesList.Where(type => typeof(TType).IsAssignableFrom(type));
    }

    public static IEnumerable<Type> TypesDerivedFrom<TInterface, TType>(this IEnumerable<Type> typesList)
    {
        return typesList.Where(type => typeof(TInterface).IsAssignableFrom(type) && typeof(TType).IsAssignableFrom(type));
    }
}