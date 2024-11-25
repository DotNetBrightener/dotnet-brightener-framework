#nullable enable
using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    private static readonly OperatorComparer[] DateTimeSupportedOperators =
    [
        OperatorComparer.In,
        OperatorComparer.NotIn,
        OperatorComparer.GreaterThan,
        OperatorComparer.GreaterThanOrEqual,
        OperatorComparer.LessThan,
        OperatorComparer.LessThanOrEqual
    ];

    private static Expression<Func<TIn, bool>>? BuildDateTimePredicateQuery<TIn>(string[]         filterValues,
                                                                                 PropertyPathInfo property)
        where TIn : class
    {
        Expression<Func<TIn, bool>>? predicateQuery = null;

        foreach (var value in filterValues)
        {
            var (operation, filterValueActualSegment) = GetComparisonOperations(value);

            if (operation == null)
                continue;

            if (!DateTimeSupportedOperators.Contains(operation!.Value))
            {
                throw new
                    InvalidOperationException($"Operator {operation} is not supported for filtering by property '{property.Path}' of type {property.PropertyUnderlyingType.Name}");
            }

            if (!DateTime.TryParse(filterValueActualSegment, out var filterValue))
            {
                throw new InvalidOperationException("Date format cannot be recognized.");
            }
            
            var subPredicateQuery = operation.Value switch
            {
                OperatorComparer.In =>
                    Build_DateTime_In_PredicateQuery<TIn>(property, filterValueActualSegment),
                OperatorComparer.NotIn =>
                    Build_DateTime_NotIn_PredicateQuery<TIn>(property, filterValueActualSegment),
                _ =>
                    ExpressionExtensions.BuildPredicate<TIn>(filterValue, operation!.Value, property.Path)
            };


            predicateQuery = predicateQuery != null ? predicateQuery.And(subPredicateQuery) : subPredicateQuery;
        }

        return predicateQuery;
    }


    private static Expression<Func<TIn, bool>> Build_DateTime_In_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                     string
                                                                                         filterValueActualSegment)
        where TIn : class
    {
        var datesSegments = filterValueActualSegment.Split(',');

        if (datesSegments.Length != 2)
            throw new InvalidOperationException("IN/NOT IN operators need start and end date parameters.");

        if (!DateTime.TryParse(datesSegments[0], out var startValue) ||
            !DateTime.TryParse(datesSegments[1], out var endValue))
        {
            throw new InvalidOperationException("Date format cannot be recognized.");
        }

        if (startValue > endValue)
            throw new InvalidOperationException("Start date must be before end date.");

        Expression<Func<TIn, bool>>? subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(startValue,
                                                                                                  OperatorComparer
                                                                                                     .GreaterThanOrEqual,
                                                                                                  property.Path);

        subPredicateQuery =
            subPredicateQuery.And(ExpressionExtensions.BuildPredicate<TIn>(endValue,
                                                                           OperatorComparer.LessThanOrEqual,
                                                                           property.Path));

        return subPredicateQuery;
    }


    private static Expression<Func<TIn, bool>> Build_DateTime_NotIn_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                        string
                                                                                            filterValueActualSegment)
        where TIn : class
    {
        var datesSegments = filterValueActualSegment.Split(',');

        if (datesSegments.Length != 2)
            throw new InvalidOperationException("IN/NOT IN operators need start and end date parameters.");

        if (!DateTime.TryParse(datesSegments[0], out var startValue) ||
            !DateTime.TryParse(datesSegments[1], out var endValue))
        {
            throw new InvalidOperationException("Date format cannot be recognized.");
        }

        if (startValue > endValue)
            throw new InvalidOperationException("Start date must be before end date.");

        Expression<Func<TIn, bool>>? subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(startValue,
                                                                                                  OperatorComparer
                                                                                                     .LessThan,
                                                                                                  property.Path);

        subPredicateQuery =
            subPredicateQuery.Or(ExpressionExtensions.BuildPredicate<TIn>(endValue,
                                                                          OperatorComparer.GreaterThan,
                                                                          property.Path));

        return subPredicateQuery;
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