using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{

    private static Expression<Func<TIn, bool>> BuildStringPredicateQuery<TIn>(string           filterValue,
                                                                              PropertyPathInfo property)
        where TIn : class
    {
        var filterWholeValue = filterValue;

        var filterWithOperation = filterWholeValue.Split(new[]
                                                         {
                                                             '_', '(', ')'
                                                         },
                                                         StringSplitOptions.RemoveEmptyEntries |
                                                         StringSplitOptions.TrimEntries);

        OperatorComparer operation = OperatorComparer.Equals;

        if (filterWithOperation.Length == 2)
        {
            filterWholeValue = filterWithOperation[1];

            operation = filterWithOperation[0].ToCompareOperator(false) ?? OperatorComparer.Equals;
        }

        var escapedFilterValue = filterWholeValue.Replace("*", "");

        object filterValues = escapedFilterValue;

        if (operation == OperatorComparer.In ||
            operation == OperatorComparer.NotIn)
        {
            filterValues = escapedFilterValue
                          .ToLower()
                          .Split(new[]
                                 {
                                     ',', ';'
                                 },
                                 StringSplitOptions.RemoveEmptyEntries |
                                 StringSplitOptions.TrimEntries)
                          .Select(value => value.Trim())
                          .ToList();
        }
        else
        {
            if (filterWholeValue.StartsWith("*") &&
                filterWholeValue.EndsWith("*"))
            {
                operation = OperatorComparer.Contains;
            }

            else if (filterWholeValue.EndsWith("*"))
            {
                operation = OperatorComparer.StartsWith;
            }

            else if (filterWholeValue.StartsWith("*"))
            {
                operation = OperatorComparer.EndsWith;
            }
        }

        var predicateQuery =
            ExpressionExtensions.BuildPredicate<TIn>(filterValues,
                                                     operation,
                                                     property.Path);

        return predicateQuery;
    }
}