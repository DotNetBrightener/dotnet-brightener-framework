using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.DataAccess.EF.Interceptors;

namespace DotNetBrightener.DataAccess.EF.Internal;

internal static class AuditEntityDebugViewBuilder
{
    public static void PrepareDebugView(this AuditEntity auditEntity)
    {
        var stackTrace = new StackTrace(0, true);

        var stackFrames = stackTrace.GetFrames();

        var stringBuilder = new List<string>();

        foreach (var frame in stackFrames)
        {
            var method        = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            
            if (declaringType is null)
                continue;
            
            var namespaceName = declaringType.Namespace;

            if (namespaceName is null ||
                namespaceName.StartsWith("Microsoft.EntityFrameworkCore") ||
                namespaceName.StartsWith("System"))
                continue;

            var className     = declaringType.FullName;
            var methodName    = method.Name;
            var lineNumber    = frame.GetFileLineNumber();

            if (className is null || 
                className.Contains(nameof(AuditEnabledSavingChangesInterceptor)) ||
                className.Contains(nameof(AuditEntityDebugViewBuilder)))
                continue;

            stringBuilder.Add($"{className}->{methodName}@{lineNumber}");

            if (stringBuilder.Count == 15)
            {
                break; // Stop once 15 frames have been added
            }
        }

        auditEntity.DebugView = string.Join(Environment.NewLine, stringBuilder);
    }
}

public static class QueryableExtensions
{
    public static Dictionary<string, object> ExtractFilters<T>(this IQueryable<T> query)
    {
        var whereCallExpression = query.Expression as MethodCallExpression;
        if (whereCallExpression == null || whereCallExpression.Method.Name != "Where")
            throw new InvalidOperationException("The query does not contain a Where clause.");

        var lambdaExpression = (LambdaExpression)((UnaryExpression)whereCallExpression.Arguments[1]).Operand;
        return ParseExpression(lambdaExpression.Body);
    }

    private static Dictionary<string, object> ParseExpression(Expression expression)
    {
        var result = new Dictionary<string, object>();

        if (expression is BinaryExpression binaryExpression)
        {
            // Handle Equal, GreaterThan, LessThan, etc.
            var member        = binaryExpression.Left as MemberExpression;
            var constantValue = GetValueFromExpression(binaryExpression.Right);

            if (member != null)
            {
                result.Add(member.Member.Name, $"{binaryExpression.NodeType.ToString()}({constantValue})");
            }

            // Handle logical expressions with AndAlso/OrElse
            if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse)
            {
                var leftResult  = ParseExpression(binaryExpression.Left);
                var rightResult = ParseExpression(binaryExpression.Right);

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
            // Handle StartsWith, EndsWith, Contains, etc.
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
        else if (expression is UnaryExpression unaryExpression)
        {
            // Handle cases like conversions or not expressions
            return ParseExpression(unaryExpression.Operand);
        }
        else if (expression is MemberExpression memberExpression && memberExpression.Expression.NodeType == ExpressionType.Parameter)
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
            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
        }
        catch (Exception)
        {
            return null;
        }
    }
}