using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotNetBrightener.Core.Utils
{
    public enum ExpressionOperators
    {
        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        LessThan,
        LessThanOrEqual
    }

    public enum ConditionOperator
    {
        And,
        Or
    }

    public static class ExpressionBuilderUtils
    {
        private static readonly Dictionary<ExpressionOperators, Func<Expression, Expression, BinaryExpression>>
            ComparisonOperationsMapping
                = new Dictionary<ExpressionOperators, Func<Expression, Expression, BinaryExpression>>
                {
                    {ExpressionOperators.Equal, Expression.Equal},
                    {ExpressionOperators.GreaterThan, Expression.GreaterThan},
                    {ExpressionOperators.GreaterThanOrEqual, Expression.GreaterThanOrEqual},
                    {ExpressionOperators.LessThan, Expression.LessThan},
                    {ExpressionOperators.LessThanOrEqual, Expression.LessThanOrEqual}
                };

        private static readonly Dictionary<ConditionOperator, Func<Expression, Expression, BinaryExpression>>
            ConditionOperatorsMapping
                = new Dictionary<ConditionOperator, Func<Expression, Expression, BinaryExpression>>
                {
                    {ConditionOperator.And, Expression.And},
                    {ConditionOperator.Or, Expression.Or}
                };

        public static Expression<Func<T, bool>> BuildExpression<T, TCompareType>(string              columnName,
                                                                                 TCompareType        compareValue,
                                                                                 ExpressionOperators comparison)
        {
            var targetType = typeof(T);

            var nestedProperties = columnName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

            var param = Expression.Parameter(targetType);
            Expression leftOperand = null;

            if (nestedProperties.Length == 1)
            {
                leftOperand = Expression.Property(param, columnName);
            }
            else
            {
                foreach (var nestedProperty in nestedProperties)
                {
                    if (leftOperand == null) // first access to the property, pick the property from the type itself
                    {
                        leftOperand = Expression.Property(param, nestedProperty);
                    }
                    else // second+ access, pick the property from the nested ones
                    {
                        leftOperand = Expression.Property(leftOperand, nestedProperty);
                    }
                }
            }

            // TODO: Validate if the property access can be evaluated in run-time
            // TODO: Validate if the property type is correct
            //if (lookingProp == null)
            //    throw new
            //        InvalidOperationException("Error while trying to build the expression with a non-existed column, or the column data type does not match requested comparison value");


            var rightOperand = Expression.Constant(compareValue, typeof(TCompareType));

            if (!ComparisonOperationsMapping.TryGetValue(comparison, out var comparisonOperation))
                throw new NotSupportedException();

            var expression =
                Expression.Lambda<Func<T, bool>>(comparisonOperation(leftOperand, rightOperand), param);

            return expression;
        }

        public static Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> leftOperand,
                                                                      Expression<Func<T, bool>> rightOperand,
                                                                      ConditionOperator         conditionOperator)
        {
            if (!ConditionOperatorsMapping.TryGetValue(conditionOperator, out var combineExpression))
                throw new NotSupportedException();

            var expression =
                Expression.Lambda<Func<T, bool>>(
                                                 combineExpression(
                                                                   // need to evaluate the 2 expression with the same object,
                                                                   // so we use left operand param for both calls here
                                                                   Expression.Invoke(leftOperand,
                                                                                     leftOperand.Parameters),
                                                                   Expression.Invoke(rightOperand,
                                                                                     leftOperand.Parameters)
                                                                  ),
                                                 leftOperand.Parameters);

            return expression;
        }
    }
}