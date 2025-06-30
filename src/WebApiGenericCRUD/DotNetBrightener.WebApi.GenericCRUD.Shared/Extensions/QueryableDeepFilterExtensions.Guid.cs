using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    private static readonly List<OperatorComparer> SupportedGuidOperators =
    [
        OperatorComparer.In,
        OperatorComparer.NotIn,
        OperatorComparer.Equals,
        OperatorComparer.NotEqual
    ];

    private static Expression<Func<TIn, bool>> BuildGuidPredicateQuery<TIn>(string           filterValue,
                                                                            PropertyPathInfo property)
        where TIn : class
    {
        var filterWholeValue = filterValue;

        var filterWithOperation = filterWholeValue.Split([
                                                             '_', '(', ')'
                                                         ],
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
                          .Split([
                                     ',', ';'
                                 ],
                                 StringSplitOptions.RemoveEmptyEntries |
                                 StringSplitOptions.TrimEntries)
                          .Select(value => Guid.TryParse(value, out var guidValue) ? guidValue : Guid.Empty)
                          .Where(_ => _ != Guid.Empty)
                          .ToList();

            if (property.IsNullable)
            {
                filterValues = new List<Guid?>(((List<Guid>)filterValues).Select(_ => (Guid?)_));
            }
        }
        else
        {
            filterValues = Guid.TryParse(escapedFilterValue, out var guidValue)
                               ? guidValue
                               : throw new InvalidOperationException($"Guid value cannot be recognized.");
        }

        if (!SupportedGuidOperators.Contains(operation))
        {
            operation = OperatorComparer.Equals;
        }

        var predicateQuery =
            ExpressionExtensions.BuildPredicate<TIn>(filterValues,
                                                     operation,
                                                     property.Path);

        return predicateQuery;
    }
}