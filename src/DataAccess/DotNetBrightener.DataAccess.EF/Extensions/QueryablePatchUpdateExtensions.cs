using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Extensions;

public static class QueryablePatchUpdateExtensions
{
    public static Task<int> ExecutePatchUpdateAsync<TSource>(this IQueryable<TSource> source,
                                                             Action<SetPropertyBuilder<TSource>> setPropertyBuilder,
                                                             CancellationToken ct = default)
    {
        var builder = new SetPropertyBuilder<TSource>();
        setPropertyBuilder.Invoke(builder);

        return source.ExecuteUpdateAsync(builder.SetPropertyCalls, ct);
    }

    public static Task<int> ExecutePatchUpdateAsync<TSource>(this IQueryable<TSource> source,
                                                             SetPropertyBuilder<TSource> setPropertyBuilder,
                                                             CancellationToken ct = default)
    {
        return source.ExecuteUpdateAsync(setPropertyBuilder.SetPropertyCalls, ct);
    }

    public static int ExecutePatchUpdate<TSource>(this IQueryable<TSource> source,
                                                  Action<SetPropertyBuilder<TSource>> setPropertyBuilder)
    {
        var builder = new SetPropertyBuilder<TSource>();
        setPropertyBuilder.Invoke(builder);

        return source.ExecuteUpdate(builder.SetPropertyCalls);
    }

    public static int ExecutePatchUpdate<TSource>(this IQueryable<TSource> source,
                                                  SetPropertyBuilder<TSource> setPropertyBuilder)
    {
        return source.ExecuteUpdate(setPropertyBuilder.SetPropertyCalls);
    }
}