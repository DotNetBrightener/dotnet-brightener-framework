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

                // TODO: Add exception in case of using ON / NOT ON operators. DateTime cannot be used with these operators due to lack of tz info.

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

                if (operation.Value == OperatorComparer.On)
                {
                    if (filterValue.Hour != 0 ||
                        filterValue.Minute != 0 ||
                        filterValue.Second != 0)
                    {
                        throw new
                            InvalidOperationException($"For ON / NOT ON operators, the date value must be at 00:00:00");
                    }

                    subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                 OperatorComparer.GreaterThanOrEqual,
                                                                                 property.Path);

                    subPredicateQuery =
                        subPredicateQuery.And(ExpressionExtensions.BuildPredicate<TIn>(filterValue.AddDays(1),
                                                                                       OperatorComparer.LessThan,
                                                                                       property.Path));
                }
                else if (operation.Value == OperatorComparer.NotOn)
                {
                    if (filterValue.Hour != 0 ||
                        filterValue.Minute != 0 ||
                        filterValue.Second != 0)
                    {
                        throw new
                            InvalidOperationException($"For ON / NOT ON operators, the date value must be at 00:00:00");
                    }

                    subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                 OperatorComparer.LessThan,
                                                                                 property.Path);

                    subPredicateQuery =
                        subPredicateQuery.Or(ExpressionExtensions.BuildPredicate<TIn>(filterValue.AddDays(1),
                                                                                      OperatorComparer
                                                                                         .GreaterThanOrEqual,
                                                                                      property.Path));
                }
                else
                {
                    subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                 operation!.Value,
                                                                                 property.Path);
                }

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
