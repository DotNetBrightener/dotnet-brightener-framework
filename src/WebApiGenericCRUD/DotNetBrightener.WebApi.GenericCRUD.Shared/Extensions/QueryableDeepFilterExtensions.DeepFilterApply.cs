using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Utils;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    /// <summary>
    ///     Add addition query into initial one to retrieve only provided columns from query string
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <param name="columnsToPick"></param>
    /// <returns>The new query with additional operation, if any</returns>
    public static IQueryable PerformColumnsSelectorQuery<TIn>(this IQueryable<TIn> entitiesQuery,
                                                              List<string> columnsToPick = null) where TIn : class
    {
        if (columnsToPick is null ||
            columnsToPick.Count == 0)
        {
            return entitiesQuery;
        }

        var alwaysIgnoreColumns = typeof(TIn).GetIgnoredProperties();

        var columnsToReturn = columnsToPick.Except(alwaysIgnoreColumns)
                                           .ToArray();

        if (typeof(IBaseEntity).IsAssignableFrom(typeof(TIn)) &&
            columnsToReturn.All(columnName =>
                                    !columnName.Equals(nameof(BaseEntity.Id), StringComparison.OrdinalIgnoreCase)))
        {
            columnsToReturn = new[]
                {
                    nameof(BaseEntity.Id)
                }.Concat(columnsToReturn)
                 .Distinct()
                 .ToArray();
        }

        var filteredResult =
            entitiesQuery.Select(columnsToReturn.ToDtoSelectorExpression<TIn>());

        return filteredResult;
    }
}