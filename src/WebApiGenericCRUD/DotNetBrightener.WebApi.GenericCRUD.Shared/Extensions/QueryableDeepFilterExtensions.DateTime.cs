using System;
using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    private static Expression<Func<TIn, bool>> BuildDateTimePredicateQuery<TIn>(string[]         filterValues,
                                                                                Type             propertyUnderlingType,
                                                                                PropertyPathInfo property)
        where TIn : class
    {
        Expression<Func<TIn, bool>> predicateQuery = null;

        foreach (var value in filterValues)
        {
            var (operation, filterValueActualSegment) = GetComparisonOperations(value);

            if (operation == null)
                continue;

            Expression<Func<TIn, bool>> subPredicateQuery = null;

            if (propertyUnderlingType == typeof(DateTime))
            {
                if (!DateTime.TryParse(filterValueActualSegment, out var filterValue))
                {
                    throw new InvalidOperationException($"Date format cannot be recognized.");
                }

                subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                             operation!.Value,
                                                                             property.Path);
            }
            else
            {
                if (!DateTimeOffset.TryParse(filterValueActualSegment, out var filterValue))
                {
                    throw new InvalidOperationException($"Date format cannot be recognized.");
                }

                subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                             operation!.Value,
                                                                             property.Path);

            }

            predicateQuery = predicateQuery != null ? predicateQuery.And(subPredicateQuery) : subPredicateQuery;
        }

        return predicateQuery;
    }


    private static (OperatorComparer? comparisonOperations, string operandValue)
        GetComparisonOperations(string filterValue)
    {
        var segments = filterValue.Split(new[]
                                         {
                                             '_', '(', ')'
                                         },
                                         StringSplitOptions.RemoveEmptyEntries |
                                         StringSplitOptions.TrimEntries);

        if (segments.Length == 1)
        {
            return (OperatorComparer.Equals, filterValue);
        }

        var comparisonOperations = segments[0].ToCompareOperator(false);

        return (comparisonOperations, segments[^1]);
    }
}
