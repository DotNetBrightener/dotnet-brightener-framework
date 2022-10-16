using System.Collections.Generic;

namespace System.Linq.Expressions;

internal enum OperatorComparer
{
    Contains,
    StartsWith,
    EndsWith,
    Equals             = ExpressionType.Equal,
    GreaterThan        = ExpressionType.GreaterThan,
    GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual,
    LessThan           = ExpressionType.LessThan,
    LessThanOrEqual    = ExpressionType.LessThan,
    NotEqual           = ExpressionType.NotEqual
}

internal static class OperatorComparerMapping
{
    private static readonly Dictionary<string, OperatorComparer> Mapping = 
        new()
        {
            {"eq", OperatorComparer.Equals},
            {"ne", OperatorComparer.NotEqual},
            {"lt", OperatorComparer.LessThan},
            {"le", OperatorComparer.LessThanOrEqual},
            {"gt", OperatorComparer.GreaterThan},
            {"ge", OperatorComparer.GreaterThanOrEqual},
            {"sw", OperatorComparer.StartsWith},
            {"startsWith", OperatorComparer.StartsWith},
            {"ew", OperatorComparer.EndsWith},
            {"endsWith", OperatorComparer.EndsWith},
            {"like", OperatorComparer.Contains},
            {"contains", OperatorComparer.Contains},
        };

    public static OperatorComparer ToCompareOperator(this string operatorString)
    {
        if (Mapping.TryGetValue(operatorString, out var operatorComparer))
        {
            return operatorComparer;
        }

        throw new InvalidOperationException($"Unsupported operator '{operatorString}'");
    }
}