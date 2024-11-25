#nullable enable
using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    private static readonly OperatorComparer[] DateTimeOffsetSupportedOperators =
    [
        OperatorComparer.On,
        OperatorComparer.NotOn,
        OperatorComparer.In,
        OperatorComparer.NotIn,
        OperatorComparer.GreaterThan,
        OperatorComparer.GreaterThanOrEqual,
        OperatorComparer.LessThan,
        OperatorComparer.LessThanOrEqual
    ];

    private static Expression<Func<TIn, bool>>? BuildDateTimeOffsetPredicateQuery<TIn>(string[]         filterValues,
                                                                                       PropertyPathInfo property)
        where TIn : class
    {
        Expression<Func<TIn, bool>>? predicateQuery = null;

        foreach (var value in filterValues)
        {
            var (operation, filterValueActualSegment) = GetComparisonOperations(value);

            if (operation == null)
                continue;

            if (!DateTimeOffsetSupportedOperators.Contains(operation!.Value))
                throw new
                    InvalidOperationException($"Operator {operation} is not supported for filtering by property '{property.Path}' of type {property.PropertyUnderlyingType.Name}");

            if (!DateTimeOffset.TryParse(filterValueActualSegment, out var filterValue) &&
                operation.Value != OperatorComparer.In &&
                operation.Value != OperatorComparer.NotIn)
            {
                throw new InvalidOperationException("Date format cannot be recognized.");
            }

            if (operation.Value == OperatorComparer.On ||
                operation.Value == OperatorComparer.NotOn)
            {
                VerifyTimezoneOffsetAvailable(filterValueActualSegment, filterValue);
            }

            var subPredicateQuery = operation.Value switch
            {
                OperatorComparer.In =>
                    Build_DateTimeOffset_In_PredicateQuery<TIn>(property, filterValueActualSegment),
                OperatorComparer.NotIn =>
                    Build_DateTimeOffset_NotIn_PredicateQuery<TIn>(property, filterValueActualSegment),
                OperatorComparer.On =>
                    Build_DateTimeOffset_On_PredicateQuery<TIn>(property, filterValue),
                OperatorComparer.NotOn =>
                    Build_DateTimeOffset_NotOn_PredicateQuery<TIn>(property, filterValue),
                _ => ExpressionExtensions.BuildPredicate<TIn>(filterValue, operation!.Value, property.Path)
            };

            predicateQuery = predicateQuery != null ? predicateQuery.And(subPredicateQuery) : subPredicateQuery;
        }

        return predicateQuery;
    }

    private static void VerifyTimezoneOffsetAvailable(string         filterValueActualSegment,
                                                      DateTimeOffset expectedValue)
    {
        var expectedDateIsoWithTz = expectedValue.ToString("O");
        var timezoneInfo          = expectedDateIsoWithTz.Substring(expectedDateIsoWithTz.Length - 6);

        if (!filterValueActualSegment.EndsWith(timezoneInfo))
        {
            throw new InvalidOperationException("No timezone info provided");
        }
    }

    private static Expression<Func<TIn, bool>> Build_DateTimeOffset_On_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                           DateTimeOffset   filterValue)
        where TIn : class
    {
        if (filterValue.Hour != 0 ||
            filterValue.Minute != 0 ||
            filterValue.Second != 0)
        {
            throw new
                InvalidOperationException("For ON / NOT ON operators, the date value must be at 00:00:00");
        }

        Expression<Func<TIn, bool>>? subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                                  OperatorComparer
                                                                                                     .GreaterThanOrEqual,
                                                                                                  property.Path);

        subPredicateQuery =
            subPredicateQuery.And(ExpressionExtensions.BuildPredicate<TIn>(filterValue.AddDays(1),
                                                                           OperatorComparer.LessThan,
                                                                           property.Path));

        return subPredicateQuery;
    }

    private static Expression<Func<TIn, bool>> Build_DateTimeOffset_NotOn_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                             DateTimeOffset   filterValue)
        where TIn : class
    {
        if (filterValue.Hour != 0 ||
            filterValue.Minute != 0 ||
            filterValue.Second != 0)
        {
            throw new
                InvalidOperationException("For ON / NOT ON operators, the date value must be at 00:00:00");
        }

        Expression<Func<TIn, bool>>? subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                                  OperatorComparer
                                                                                                     .LessThan,
                                                                                                  property.Path);

        subPredicateQuery =
            subPredicateQuery.Or(ExpressionExtensions.BuildPredicate<TIn>(filterValue.AddDays(1),
                                                                          OperatorComparer
                                                                             .GreaterThanOrEqual,
                                                                          property.Path));

        return subPredicateQuery;
    }

    private static Expression<Func<TIn, bool>> Build_DateTimeOffset_In_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                           string
                                                                                               filterValueActualSegment)
        where TIn : class
    {
        var datesSegments = filterValueActualSegment.Split(',');

        if (datesSegments.Length != 2)
            throw new InvalidOperationException("IN/NOT IN operators need start and end date parameters.");

        if (!DateTimeOffset.TryParse(datesSegments[0], out var startValue) ||
            !DateTimeOffset.TryParse(datesSegments[1], out var endValue))
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


    private static Expression<Func<TIn, bool>> Build_DateTimeOffset_NotIn_PredicateQuery<TIn>(PropertyPathInfo property,
                                                                                              string
                                                                                                  filterValueActualSegment)
        where TIn : class
    {
        var datesSegments = filterValueActualSegment.Split(',');

        if (datesSegments.Length != 2)
            throw new InvalidOperationException("IN/NOT IN operators need start and end date parameters.");

        if (!DateTimeOffset.TryParse(datesSegments[0], out var startValue) ||
            !DateTimeOffset.TryParse(datesSegments[1], out var endValue))
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