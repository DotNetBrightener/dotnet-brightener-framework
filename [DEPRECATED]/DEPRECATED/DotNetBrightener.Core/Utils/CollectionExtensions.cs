﻿using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Core.Utils;

public static class CollectionExtensions
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}