using System.Linq.Expressions;
using LinqToDB;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<TResult> LeftJoin<T, TOther, TResult>(this IQueryable<T> thisQuery,
                                                                       IQueryable<TOther> otherQuery,
                                                                       Expression<Func<T, TOther, bool>>
                                                                           joinCondition,
                                                                       Expression<Func<T, TOther, TResult>>
                                                                           resultSelector)
        {
            return LinqExtensions.LeftJoin(thisQuery, otherQuery, joinCondition, resultSelector);
        }
    }
}