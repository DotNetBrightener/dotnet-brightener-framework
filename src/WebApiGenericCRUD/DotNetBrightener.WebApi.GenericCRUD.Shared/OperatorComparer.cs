using System.Collections.Generic;

namespace System.Linq.Expressions;

internal enum OperatorComparer
{
    Equals             = ExpressionType.Equal,
    GreaterThan        = ExpressionType.GreaterThan,
    GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual,
    LessThan           = ExpressionType.LessThan,
    LessThanOrEqual    = ExpressionType.LessThanOrEqual,
    NotEqual           = ExpressionType.NotEqual,

    Contains,
    NotContains,
    StartsWith,
    NotStartsWith,
    EndsWith,
    NotEndsWith,
    In,
    NotIn,
}

internal static class OperatorComparerMapping
{
    private static readonly Dictionary<string, OperatorComparer> Mapping =
        new()
        {
            {
                "eq", OperatorComparer.Equals
            },
            {
                "equal", OperatorComparer.Equals
            },
            {
                "Equal", OperatorComparer.Equals
            },
            {
                "=", OperatorComparer.Equals
            },
            {
                "ne", OperatorComparer.NotEqual
            },
            {
                "!=", OperatorComparer.NotEqual
            },
            {
                "!", OperatorComparer.NotEqual
            },
            {
                "lt", OperatorComparer.LessThan
            },
            {
                "<", OperatorComparer.LessThan
            },
            {
                "le", OperatorComparer.LessThanOrEqual
            },
            {
                "<=", OperatorComparer.LessThanOrEqual
            },
            {
                "gt", OperatorComparer.GreaterThan
            },
            {
                ">", OperatorComparer.GreaterThan
            },
            {
                "ge", OperatorComparer.GreaterThanOrEqual
            },
            {
                ">=", OperatorComparer.GreaterThanOrEqual
            },
            {
                "sw", OperatorComparer.StartsWith
            },
            {
                "!sw", OperatorComparer.NotStartsWith
            },
            {
                "startsWith", OperatorComparer.StartsWith
            },
            {
                "!startsWith", OperatorComparer.NotStartsWith
            },
            {
                "ew", OperatorComparer.EndsWith
            },
            {
                "!ew", OperatorComparer.NotEndsWith
            },
            {
                "endsWith", OperatorComparer.EndsWith
            },
            {
                "!endsWith", OperatorComparer.NotEndsWith
            },
            {
                "like", OperatorComparer.Contains
            },
            {
                "notlike", OperatorComparer.NotContains
            },
            {
                "!like", OperatorComparer.NotContains
            },
            {
                "contains", OperatorComparer.Contains
            },
            {
                "notcontains", OperatorComparer.NotContains
            },
            {
                "!contains", OperatorComparer.NotContains
            },
            {
                "in", OperatorComparer.In
            },
            {
                "notin", OperatorComparer.NotIn
            },
            {
                "!in", OperatorComparer.NotIn
            },
        };

    public static OperatorComparer? ToCompareOperator(this string operatorString, bool shouldThrow = true)
    {
        if (Mapping.TryGetValue(operatorString, out var operatorComparer))
        {
            return operatorComparer;
        }

        if (Enum.TryParse(operatorString, ignoreCase: true, out operatorComparer))
        {
            return operatorComparer;
        }

        if (!shouldThrow)
            return null;

        throw new InvalidOperationException($"Unsupported operator '{operatorString}'");
    }
}