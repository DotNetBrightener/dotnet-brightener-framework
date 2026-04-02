#nullable enable
using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    private static Expression<Func<TIn, bool>>? BuildDateOnlyPredicateQuery<TIn>(string[]         filterValues,
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

            // Route In/NotIn first — they handle their own range parsing internally
            var subPredicateQuery = operation.Value switch
            {
                OperatorComparer.In =>
                    Build_DateOnly_In_PredicateQuery<TIn>(property, filterValueActualSegment),
                OperatorComparer.NotIn =>
                    Build_DateOnly_NotIn_PredicateQuery<TIn>(property, filterValueActualSegment),
                _ =>
                    ParseDateOnlyAndBuildPredicate<TIn>(filterValueActualSegment, operation!.Value, property.Path)
            };

            predicateQuery = predicateQuery != null ? predicateQuery.And(subPredicateQuery) : subPredicateQuery;
        }

        return predicateQuery;
    }

    private static Expression<Func<TIn, bool>> ParseDateOnlyAndBuildPredicate<TIn>(
        string           rawValue,
        OperatorComparer operation,
        string           propertyPath)
        where TIn : class
    {
        if (!DateOnly.TryParse(rawValue, out var filterValue))
        {
            throw new InvalidOperationException("Date format cannot be recognized.");
        }

        return ExpressionExtensions.BuildPredicate<TIn>(filterValue, operation, propertyPath);
    }


    private static Expression<Func<TIn, bool>> Build_DateOnly_In_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                     string
                                                                                         filterValueActualSegment)
        where TIn : class
    {
        var datesSegments = filterValueActualSegment.Split([',', ';']);

        if (datesSegments.Length != 2)
            throw new InvalidOperationException("IN/NOT IN operators need start and end date parameters.");

        if (!DateOnly.TryParse(datesSegments[0].Trim(), out var startValue) ||
            !DateOnly.TryParse(datesSegments[1].Trim(), out var endValue))
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


    private static Expression<Func<TIn, bool>> Build_DateOnly_NotIn_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                        string
                                                                                            filterValueActualSegment)
        where TIn : class
    {
        var datesSegments = filterValueActualSegment.Split([',', ';']);

        if (datesSegments.Length != 2)
            throw new InvalidOperationException("IN/NOT IN operators need start and end date parameters.");

        if (!DateOnly.TryParse(datesSegments[0].Trim(), out var startValue) ||
            !DateOnly.TryParse(datesSegments[1].Trim(), out var endValue))
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
}