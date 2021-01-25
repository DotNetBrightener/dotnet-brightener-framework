using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Linq
{
    public static class ExpressionToCacheKeyExtension
    {
        public static string GenerateCacheKey<TObject>(this Expression<Func<TObject, bool>> expression)
        {
            if (expression.Body is not BinaryExpression methodBinaryExpression)
            {
                return "";
            }

            var stringBuilder = new StringBuilder();

            ProcessExpression(methodBinaryExpression, stringBuilder);

            var result = stringBuilder.ToString();

            return typeof(TObject).Name + "_" + result;
        }

        private static void ProcessExpression(Expression expression, StringBuilder stringBuilder)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                stringBuilder.Append("[");
                ProcessExpression(binaryExpression.Left, stringBuilder);

                if (binaryExpression.NodeType == ExpressionType.Equal)
                {
                    stringBuilder.Append("==");
                }
                else if (binaryExpression.NodeType == ExpressionType.NotEqual)
                {
                    stringBuilder.Append("!=");
                }
                else if (binaryExpression.NodeType == ExpressionType.LessThan)
                {
                    stringBuilder.Append("<");
                }
                else if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual)
                {
                    stringBuilder.Append("<=");
                }
                else if (binaryExpression.NodeType == ExpressionType.GreaterThan)
                {
                    stringBuilder.Append(">");
                }
                else if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual)
                {
                    stringBuilder.Append(">=");
                }
                else if (binaryExpression.NodeType == ExpressionType.And || 
                         binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    stringBuilder.Append("&&");
                } 
                else if (binaryExpression.NodeType == ExpressionType.Or || 
                         binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    stringBuilder.Append("||");
                }

                ProcessExpression(binaryExpression.Right, stringBuilder);
                stringBuilder.Append("]");
            }

            else if (expression is MemberExpression memberExpression)
            {
                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    if (memberExpression.Expression.NodeType == ExpressionType.Constant && 
                        memberExpression.Expression is ConstantExpression constantExpression)
                    {
                        var value = (memberExpression.Member as FieldInfo).GetValue(constantExpression.Value);
                        stringBuilder.Append(value);
                    }
                    else
                    {
                        stringBuilder.Append(memberExpression.Member.Name);
                    }
                }
            } 
            else if (expression is ConstantExpression constantExpression)
            {
                stringBuilder.Append(
                    constantExpression.Value == null ? "null" : constantExpression.Value
                    );
            }
        }
    }
}