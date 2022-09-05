using System;
using System.Collections.Generic;

namespace DotNetBrightener.Core.Modular;

public class TypeComparisonEquality : IEqualityComparer<Type>
{
    public bool Equals(Type x, Type y)
    {
        if (x == null || y == null)
            return false;

        return x.FullName == y.FullName;
    }

    public int GetHashCode(Type obj)
    {
        return obj.GetHashCode();
    }
}