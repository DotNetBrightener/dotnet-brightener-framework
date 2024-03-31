using System;
using System.Linq;
using System.Linq.Expressions;

namespace DotNetBrightener.gRPC.Extensions;

public static class ObjectCopyExtensions
{
    public static void CopyTo<T>(this object                      source,
                                 T                                destination,
                                 bool                             copyNullValues    = false,
                                 Expression<Func<object, object>> ignoredProperties = null) =>
        System.ObjectCopyExtensions.CopyTo(source, destination, copyNullValues, ignoredProperties);
}