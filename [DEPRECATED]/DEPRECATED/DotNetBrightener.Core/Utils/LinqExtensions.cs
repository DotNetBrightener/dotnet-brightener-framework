using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Core.Utils;

public static class LinqExtensions
{
    /// <summary>
    /// LINQ Left Join
    /// </summary>
    /// <typeparam name="TSource">Right item type</typeparam>
    /// <typeparam name="TInner">Left item type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source">Left items</param>
    /// <param name="inner">Right items</param>
    /// <param name="pk">Right item key selector</param>
    /// <param name="fk">Left item key selector</param>
    /// <param name="result">Result selector</param>
    public static IEnumerable<TResult>
        LeftJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource>      source,
                                                 IEnumerable<TInner>            inner,
                                                 Func<TSource, TKey>            pk,
                                                 Func<TInner, TKey>             fk,
                                                 Func<TSource, TInner, TResult> result)
        where TSource : class where TInner : class
    {
        IEnumerable<TResult> _result = Enumerable.Empty<TResult>();

        _result = from s in source
                  join i in inner
                      on pk(s) equals fk(i) into joinData
                  from left in joinData.DefaultIfEmpty()
                  select result(s, left);

        return _result;
    }

    /// <summary>
    /// LINQ Right Join
    /// </summary>
    /// <typeparam name="TSource">Right item type</typeparam>
    /// <typeparam name="TInner">Left item type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source">Left items</param>
    /// <param name="inner">Right items</param>
    /// <param name="pk">Right item key selector</param>
    /// <param name="fk">Left item key selector</param>
    /// <param name="result">Result selector</param>
    public static IEnumerable<TResult>
        RightJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource>      source,
                                                  IEnumerable<TInner>            inner,
                                                  Func<TSource, TKey>            pk,
                                                  Func<TInner, TKey>             fk,
                                                  Func<TSource, TInner, TResult> result)
        where TSource : class where TInner : class
    {
        IEnumerable<TResult> _result = Enumerable.Empty<TResult>();

        _result = from i in inner
                  join s in source
                      on fk(i) equals pk(s) into joinData
                  from right in joinData.DefaultIfEmpty()
                  select result(right, i);

        return _result;
    }

    /// <summary>
    /// LINQ Full Outer Join
    /// </summary>
    /// <typeparam name="TSource">Right item type</typeparam>
    /// <typeparam name="TInner">Left item type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source">Left items</param>
    /// <param name="inner">Right items</param>
    /// <param name="pk">Right item key selector</param>
    /// <param name="fk">Left item key selector</param>
    /// <param name="result">Result selector</param>
    public static IEnumerable<TResult>
        FullOuterJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource>      source,
                                                      IEnumerable<TInner>            inner,
                                                      Func<TSource, TKey>            pk,
                                                      Func<TInner, TKey>             fk,
                                                      Func<TSource, TInner, TResult> result)
        where TSource : class where TInner : class
    {
        var left  = source.LeftJoin(inner, pk, fk, result).ToList();
        var right = source.RightJoin(inner, pk, fk, result).ToList();

        return left.Union(right);
    }

    /// <summary>
    /// LINQ Distinct By
    /// </summary>
    /// <typeparam name="TSource">Items type</typeparam>
    /// <typeparam name="TKey">Distinct by key type</typeparam>
    /// <param name="source">Items</param>
    /// <param name="keySelector">Distinct by key selector</param>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
}