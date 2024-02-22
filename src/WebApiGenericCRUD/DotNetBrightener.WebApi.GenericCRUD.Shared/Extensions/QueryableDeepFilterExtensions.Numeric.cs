using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    static readonly List<OperatorComparer> SupportedNumericOperators =
    [
        OperatorComparer.In,
        OperatorComparer.NotIn,
        OperatorComparer.Equals,
        OperatorComparer.NotEqual,
        OperatorComparer.LessThan,
        OperatorComparer.LessThanOrEqual,
        OperatorComparer.GreaterThan,
        OperatorComparer.GreaterThanOrEqual
    ];

    private static Expression<Func<TIn, bool>> BuildNumericPredicateQuery<TIn>(string           filterValue,
                                                                               PropertyPathInfo property)
        where TIn : class
    {
        var filterWholeValues = new List<string>();

        Expression<Func<TIn, bool>> predicateStatement = null;

        if (filterValue.StartsWith("in(", StringComparison.OrdinalIgnoreCase) || 
            filterValue.StartsWith("!in(", StringComparison.OrdinalIgnoreCase))
        {
            filterWholeValues.Add(filterValue);
        }
        else
        {
            filterWholeValues.AddRange(filterValue.Split(new[]
                                                         {
                                                             ','
                                                         },
                                                         StringSplitOptions.RemoveEmptyEntries |
                                                         StringSplitOptions.TrimEntries));
        }

        foreach (var filterWholeValue in filterWholeValues)
        {
            var wholeValue = filterWholeValue;

            var filterWithOperation = filterWholeValue.Split(new[]
                                                             {
                                                                 '_', '(', ')'
                                                             },
                                                             StringSplitOptions.RemoveEmptyEntries |
                                                             StringSplitOptions.TrimEntries);

            OperatorComparer operation = OperatorComparer.Equals;

            if (filterWithOperation.Length == 2)
            {
                wholeValue = filterWithOperation[1];

                operation = filterWithOperation[0].ToCompareOperator(false) ?? OperatorComparer.Equals;
            }

            var escapedFilterValue = wholeValue.Replace("*", "");

            object filterValues;

            if (!SupportedNumericOperators.Contains(operation))
            {
                operation = OperatorComparer.Equals;
            }

            var dynamicValues = escapedFilterValue
                               .ToLower()
                               .Split(new[]
                                      {
                                          ',', ';'
                                      },
                                      StringSplitOptions.RemoveEmptyEntries |
                                      StringSplitOptions.TrimEntries)
                               .Select(value => Regex.Replace(value, "[^0-9.]", ""))
                               .Select(_ => Convert.ChangeType(_, property.PropertyUnderlyingType))
                               .ToList();

            if (dynamicValues.Count == 0)
                return null;


            // Dynamically create a List<> of the correct type using reflection
            Type listType = typeof(List<>).MakeGenericType(property.IsNullable
                                                               ? property.NullablePropertyUnderlyingType
                                                               : property.PropertyUnderlyingType);
            object typedList = Activator.CreateInstance(listType);

            // Add the converted values to the dynamically created list
            MethodInfo addMethod = listType.GetMethod("Add");

            foreach (var value in dynamicValues)
            {
                addMethod.Invoke(typedList,
                                 new[]
                                 {
                                     value
                                 });
            }

            if (operation == OperatorComparer.In ||
                operation == OperatorComparer.NotIn)
            {
                filterValues = typedList;
            }
            else
            {
                filterValues = ((dynamic)typedList)?[0];
            }

            var predicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValues,
                                                                          operation,
                                                                          property.Path);

            predicateStatement = predicateStatement != null ? predicateStatement.And(predicateQuery) : predicateQuery;
        }

        return predicateStatement;
    }
}