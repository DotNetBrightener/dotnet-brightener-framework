using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.EF.Internal;

public static class QueryableExtensions
{
    public static Dictionary<string, object> ExtractFilters<T>(this Expression<Func<T, bool>> conditionExpression)
    {
        var result = new Dictionary<string, object>();

        // The main expression is the body of the conditionExpression
        Expression expression = conditionExpression.Body;

        if (expression is BinaryExpression binaryExpression)
        {
            // Handle Equal, GreaterThan, LessThan, etc.
            var member        = binaryExpression.Left as MemberExpression;
            var constantValue = GetValueFromExpression(binaryExpression.Right);

            if (member != null)
            {
                object value = binaryExpression.NodeType == ExpressionType.Equal
                                   ? constantValue
                                   : $"{binaryExpression.NodeType.ToString()}({constantValue})";
                result.Add(member.Member.Name, value);
            }

            // Handle logical expressions with AndAlso/OrElse
            if (binaryExpression.NodeType == ExpressionType.AndAlso ||
                binaryExpression.NodeType == ExpressionType.OrElse)
            {
                var leftResult =
                    ExtractFilters<T>(Expression.Lambda<Func<T, bool>>(binaryExpression.Left,
                                                                       conditionExpression.Parameters));
                var rightResult =
                    ExtractFilters<T>(Expression.Lambda<Func<T, bool>>(binaryExpression.Right,
                                                                       conditionExpression.Parameters));

                foreach (var kv in leftResult)
                {
                    result[kv.Key] = kv.Value;
                }

                foreach (var kv in rightResult)
                {
                    result[kv.Key] = kv.Value;
                }
            }
        }
        else if (expression is MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == "Contains" &&
                methodCallExpression.Arguments.Count == 2)
            {
                // Handle cases like someArray.Contains(x.Id)
                var collection = GetValueFromExpression(methodCallExpression.Arguments[0]);
                var member     = methodCallExpression.Arguments[1] as MemberExpression;

                if (member != null)
                {
                    result.Add(member.Member.Name, $"in({string.Join(",", ((IEnumerable<object>)collection))})");
                }
            }
            else
            {
                // Handle StartsWith, EndsWith, Contains (on string)
                var member   = methodCallExpression.Object as MemberExpression;
                var argument = (methodCallExpression.Arguments.First() as ConstantExpression)?.Value;

                if (member != null)
                {
                    switch (methodCallExpression.Method.Name)
                    {
                        case "StartsWith":
                            result.Add(member.Member.Name, $"startsWith({argument})");

                            break;
                        case "EndsWith":
                            result.Add(member.Member.Name, $"endsWith({argument})");

                            break;
                        case "Contains":
                            result.Add(member.Member.Name, $"contains({argument})");

                            break;
                        default:
                            break;
                    }
                }
            }
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            // Handle cases like conversions or not expressions
            return ExtractFilters<T>(Expression.Lambda<Func<T, bool>>(unaryExpression.Operand,
                                                                      conditionExpression.Parameters));
        }
        else if (expression is MemberExpression memberExpression &&
                 memberExpression.Expression.NodeType == ExpressionType.Parameter)
        {
            // Handle simple boolean property checks (e.g., Where(x => x.IsActive))
            result.Add(memberExpression.Member.Name, true);
        }

        return result;
    }

    private static object GetValueFromExpression(Expression expression)
    {
        try
        {
            if (expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value;
            }

            // Compile and execute the expression to get the value
            var lambda   = Expression.Lambda(expression);
            var compiled = lambda.Compile();

            return compiled.DynamicInvoke();
        }
        catch (Exception)
        {
            return null;
        }
    }
}