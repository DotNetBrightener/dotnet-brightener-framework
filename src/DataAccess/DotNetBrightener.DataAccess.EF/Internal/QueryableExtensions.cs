using System.Linq.Expressions;
using Newtonsoft.Json;

namespace DotNetBrightener.DataAccess.EF.Internal;

/*
 
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


 */

public class FilterInformation : List<object>
{
    public string Serialize(bool indented = false)
    {
        if (Count == 0)
        {
            return "[NO_FILTER_PROVIDED]";
        }

        var serializeObject = Count == 1
                                  ? JsonConvert.SerializeObject(this[0],
                                                                indented ? Formatting.Indented : Formatting.None)
                                  : JsonConvert.SerializeObject(this,
                                                                indented ? Formatting.Indented : Formatting.None);

        return serializeObject;
    }
}

public static class QueryableExtensions
{
    public static FilterInformation ExtractFilters<T>(this Expression<Func<T, bool>>? conditionExpression)
    {
        if (conditionExpression is null)
        {
            return new FilterInformation();
        }

        return ParseExpression(conditionExpression.Body);
    }

    public static FilterInformation ParseExpression(this Expression conditionExpression, int groupIndex = 0)
    {
        var result = new FilterInformation();

        Expression expression = conditionExpression;

        if (expression is BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                // Group the AND conditions
                var groupDict = new Dictionary<string, object>();

                var leftResult  = ParseExpression(binaryExpression.Left, groupIndex + 1);
                var rightResult = ParseExpression(binaryExpression.Right, groupIndex + 1);

                foreach (var item in leftResult)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        foreach (var kv in dict)
                        {
                            groupDict[kv.Key] = kv.Value;
                        }
                    }
                }

                foreach (var item in rightResult)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        foreach (var kv in dict)
                        {
                            groupDict[kv.Key] = kv.Value;
                        }
                    }
                }

                result.Add(new Dictionary<string, object>
                {
                    {
                        $"g{groupIndex}", groupDict
                    }
                });
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                // Handle OR conditions
                var leftResult  = ParseExpression(binaryExpression.Left, groupIndex + 1);
                var rightResult = ParseExpression(binaryExpression.Right, groupIndex + 1);

                result.AddRange(leftResult);
                result.Add("OR");
                result.AddRange(rightResult);
            }
            else
            {
                // Handle comparison expressions (e.g., ==, <, >)
                var member        = binaryExpression.Left as MemberExpression;
                var constantValue = GetValueFromExpression(binaryExpression.Right);

                if (member != null)
                {
                    object value = binaryExpression.NodeType == ExpressionType.Equal
                                       ? constantValue
                                       : $"{binaryExpression.NodeType.ToString()}({constantValue})";

                    result.Add(new Dictionary<string, object>
                    {
                        {
                            member.Member.Name, value
                        }
                    });
                }
            }
        }
        else if (expression is MethodCallExpression methodCallExpression)
        {
            MemberExpression? member = null;

            if (methodCallExpression.Method.Name == "Contains" &&
                methodCallExpression.Arguments.Count == 1)
            {
                // Handle cases like someArray.Contains(x.Id)
                var collection = GetValueFromExpression(methodCallExpression.Object);
                member = methodCallExpression.Arguments[0] as MemberExpression;

                if (collection is not null &&
                    member is not null)
                {
                    result.Add(new Dictionary<string, object>
                    {
                        {
                            member.Member.Name, $"in({JsonConvert.SerializeObject(collection)})"
                        }
                    });

                    return result;
                }
            }

            // Handle StartsWith, EndsWith, Contains (on string)
            member = methodCallExpression.Object as MemberExpression;
            var argument = (methodCallExpression.Arguments.First() as ConstantExpression)?.Value;

            if (member != null)
            {

                result.Add(new Dictionary<string, object>
                {
                    {
                        member.Member.Name,
                        $"{methodCallExpression.Method.Name}({JsonConvert.SerializeObject(argument)})"
                    }
                });
            }
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            return ParseExpression(unaryExpression.Operand, groupIndex);
        }
        else if (expression is MemberExpression memberExpression &&
                 memberExpression.Expression.NodeType == ExpressionType.Parameter)
        {
            result.Add(new Dictionary<string, object>
            {
                {
                    memberExpression.Member.Name, true
                }
            });
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