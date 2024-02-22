using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Reflection;
using DotNetBrightener.Framework.Exceptions;

namespace System.Linq.Expressions;

internal static class ExpressionExtensions
{
    public static IEnumerable<PropertyInfo> GetSimplePropertyAccessList(this LambdaExpression propertyAccessExpression)
    {
        var propertyPaths
            = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

        if (propertyPaths == null)
        {
            throw new InvalidOperationException();
        }

        return propertyPaths;
    }


    private static PropertyInfo MatchSimplePropertyAccess(this Expression parameterExpression,
                                                          Expression      propertyAccessExpression)
    {
        var propertyPath = MatchPropertyAccess(parameterExpression, propertyAccessExpression).ToArray();

        return propertyPath.Length == 1 ? propertyPath[0] : null;
    }

    private static IEnumerable<PropertyInfo> MatchPropertyAccessList(this LambdaExpression lambdaExpression,
                                                                     Func<Expression, Expression, PropertyInfo>
                                                                         propertyMatcher)
    {
        if (RemoveConvert(lambdaExpression.Body) is NewExpression newExpression)
        {
            var parameterExpression
                = lambdaExpression.Parameters.Single();

            var propertyPaths
                = newExpression.Arguments
                               .Select(a => propertyMatcher(a, parameterExpression))
                               .Where(p => p != null)
                               .ToArray();

            if (propertyPaths.Length == newExpression.Arguments.Count)
            {
                return newExpression.HasDefaultMembersOnly(propertyPaths) ? propertyPaths : null;
            }
        }

        var propertyPath = propertyMatcher(lambdaExpression.Body, lambdaExpression.Parameters.Single());

        return (propertyPath != null)
                   ? new[]
                   {
                       propertyPath
                   }
                   : null;
    }


    private static bool HasDefaultMembersOnly(this NewExpression        newExpression,
                                              IEnumerable<PropertyInfo> propertyPaths)
    {
        return newExpression?.Members?
                             .Where(
                                    (t, i) =>
                                        !string.Equals(t.Name,
                                                       propertyPaths.ElementAt(i).Name,
                                                       StringComparison.Ordinal))
                             .Any() == false;
    }

    public static IEnumerable<PropertyInfo> MatchPropertyAccess(
        this Expression parameterExpression,
        Expression      propertyAccessExpression)
    {
        var propertyInfos = new List<PropertyInfo>();

        MemberExpression memberExpression;

        do
        {
            memberExpression = RemoveConvert(propertyAccessExpression) as MemberExpression;

            if (memberExpression == null)
            {
                return Array.Empty<PropertyInfo>();
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                return Array.Empty<PropertyInfo>();
            }

            propertyInfos.Insert(0, propertyInfo);

            propertyAccessExpression = memberExpression.Expression;
        } while (memberExpression.Expression != parameterExpression);

        return propertyInfos;
    }


    public static Expression RemoveConvert(this Expression expression)
    {
        while ((expression != null)
            && (expression.NodeType == ExpressionType.Convert
             || expression.NodeType == ExpressionType.ConvertChecked))
        {
            expression = RemoveConvert(((UnaryExpression)expression).Operand);
        }

        return expression;
    }

    public static Expression<Func<T, object>> ToMemberAccessExpression<T>(this string fieldName) where T : class
        => BuildMemberAccessExpression<T>(fieldName);

    public static Expression<Func<T, object>> BuildMemberAccessExpression<T>(string fieldName) where T : class
    {
        var type      = typeof(T);
        var fieldInfo = type.GetMember(fieldName).FirstOrDefault();

        if (fieldInfo == null)
        {
            fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);

            fieldInfo = type.GetMember(fieldName).FirstOrDefault();
        }

        if (fieldInfo == null || !(fieldInfo is PropertyInfo propertyInfo))
            throw new UnknownPropertyException(fieldName, type);

        var        paramExpression = Expression.Parameter(type, "_");
        Expression memberAccess    = Expression.MakeMemberAccess(paramExpression, propertyInfo);


        if (propertyInfo.PropertyType.IsValueType)
        {
            memberAccess = Expression.Convert(memberAccess, typeof(object));
        }

        var memberAccessExpression = Expression.Lambda<Func<T, object>>(memberAccess, paramExpression);

        return memberAccessExpression;
    }

    public static Expression<Func<T, T>> BuildMemberInitExpressionFromDto<T>(object dataTransferObject) where T : class
    {
        Type type                 = typeof(T);
        var  newExpression        = Expression.New(type.GetConstructor(new Type[0]));
        var  memberAssignmentList = new List<MemberAssignment>();

        var jobject = JObject.FromObject(dataTransferObject);
        //var dtoType = dataTransferObject.GetType();

        // not converting some properties that should not be put back to the entity
        var sourceProps = jobject.Properties();

        foreach (var propertyInfo in sourceProps)
        {
            PropertyInfo destinationProp = type.GetProperty(propertyInfo.Name);
            if (destinationProp == null ||
                destinationProp.HasAttribute<NotMappedAttribute>())
                continue;

            var value = propertyInfo.Value.ToObject(destinationProp.PropertyType);

            memberAssignmentList.Add(Expression.Bind(
                                                     destinationProp,
                                                     Expression.Constant(value, destinationProp.PropertyType)
                                                    )
                                    );
        }

        var memberInitExpression =
            Expression
               .Lambda<Func<T, T>>(Expression.MemberInit(newExpression, memberAssignmentList),
                                   Expression.Parameter(type));

        return memberInitExpression;
    }

    public static Expression<Func<T, TOutput>> BuildMemberAccessExpression<T, TOutput>(string fieldName, string paramName = "_") where T : class
    {
        var type      = typeof(T);
        var fieldInfo = type.GetMember(fieldName).FirstOrDefault();
        if (fieldInfo == null)
        {
            fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);

            fieldInfo = type.GetMember(fieldName).FirstOrDefault();
        }

        if (fieldInfo == null || !(fieldInfo is PropertyInfo propertyInfo))
            throw new UnknownPropertyException(fieldName, type);

        var        paramExpression = Expression.Parameter(type, paramName);
        Expression memberAccess    = Expression.MakeMemberAccess(paramExpression, propertyInfo);

        var memberAccessExpression = Expression.Lambda<Func<T, TOutput>>(memberAccess, paramExpression);

        return memberAccessExpression;
    }

    public static Expression<Func<T, TOther, bool>> BuildJoinCondition<T, TOther, TOutput>(string fieldName, string otherFieldName)
        where T : class
        where TOther : class
    {
        var leftExpression  = BuildMemberAccessExpression<T, TOutput>(fieldName, "@this");
        var rightExpression = BuildMemberAccessExpression<TOther, TOutput>(otherFieldName, "@that");

        var memberAccessExpression = Expression.Equal(leftExpression.Body, rightExpression.Body);

        return Expression.Lambda<Func<T, TOther, bool>>(memberAccessExpression,
                                                        new[]
                                                        {
                                                            leftExpression.Parameters.First(),
                                                            rightExpression.Parameters.First()
                                                        });
    }

    public static Expression<Func<T, object>> BuildSelectorExpressionFromDto<T>(string[] properties)
        where T : class
    {
        if (properties == null ||
            !properties.Any())
        {
            throw new ArgumentNullException(nameof(properties), "Properties must be provided");
        }

        var source = Expression.Parameter(typeof(T), "o");
        var propertiesBinding = properties
                               .Select(propName =>
                                {
                                    var prop = typeof(T).GetProperty(propName);

                                    if (prop == null)
                                    {
                                        var csConventionName = propName[0].ToString().ToUpper() + propName.Substring(1);
                                        prop = typeof(T).GetProperty(csConventionName);
                                    }

                                    return prop;
                                })
                               .Where(_ => _ != null)
                               .Select(p => new DynamicProperty(p.Name, p.PropertyType))
                               .ToList();
        var resultType = DynamicClassFactory.CreateType(propertiesBinding, false);
        var bindings =
            propertiesBinding.Select(p => Expression.Bind(resultType.GetProperty(p.Name),
                                                          Expression.Property(source, p.Name)));
        var result = Expression.MemberInit(Expression.New(resultType), bindings);

        return Expression.Lambda<Func<T, dynamic>>(result, source);
    }

    public static Expression<Func<T, bool>> BuildPredicate<T>(object           value, 
                                                              OperatorComparer comparer, 
                                                              string           propertiesPath)
    {
        var parameterExpression = Expression.Parameter(typeof(T), typeof(T).Name);

        var properties = propertiesPath.Split(new[]
                                              {
                                                  "."
                                              },
                                              StringSplitOptions.RemoveEmptyEntries);

        return (Expression<Func<T, bool>>)BuildNavigationExpression(parameterExpression, comparer, value, properties);
    }

    /// <summary>
    ///     Build the predicate expression that can be translated
    ///     to read like <typeparamref name="T"/>.[<seealso cref="propertiesPath"/>] = <seealso cref="value"/>
    /// </summary>
    /// <typeparam name="T">The type of the target entity</typeparam>
    /// <param name="propertiesPath">
    ///     The path to the property to do the compare operation
    /// </param>
    /// <param name="value">
    ///     The other operand of the operation
    /// </param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> EqualsTo<T>(this string propertiesPath,
                                                        object      value) =>
        BuildPredicate<T>(propertiesPath, OperatorComparer.Equals, value);

    /// <summary>
    ///     Builds the predicate expression that can be translated
    ///     to read like <typeparamref name="T"/>.<seealso cref="propertiesPath"/> {comparer} {value}
    /// </summary>
    /// <typeparam name="T">The type of the target entity</typeparam>
    /// <param name="propertiesPath">
    ///     The path to the property to do the compare operation
    /// </param>
    /// <param name="comparer">The compare operation</param>
    /// <param name="value">
    ///     The other operand of the operation
    /// </param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> BuildPredicate<T>(this string      propertiesPath,
                                                              OperatorComparer comparer,
                                                              object           value)
    {
        var parameterExpression = Expression.Parameter(typeof(T), typeof(T).Name);

        var properties = propertiesPath.Split(new[]
                                              {
                                                  "."
                                              },
                                              StringSplitOptions.RemoveEmptyEntries);

        return (Expression<Func<T, bool>>)BuildNavigationExpression(parameterExpression, comparer, value, properties);
    }

    private static Expression BuildNavigationExpression(Expression       parameter, 
                                                        OperatorComparer comparer, 
                                                        object           value, 
                                                        params string[]  properties)
    {
        Expression resultExpression = null;
        Expression childParameter, predicate;
        Type       childType = null;

        if (properties.Count() > 1)
        {
            //build path
            parameter = Expression.Property(parameter, properties[0]);
            var isCollection = typeof(IEnumerable).IsAssignableFrom(parameter.Type);
            //if it´s a collection we later need to use the predicate in the methodexpressioncall
            if (isCollection)
            {
                childType      = parameter.Type.GetGenericArguments()[0];
                childParameter = Expression.Parameter(childType, childType.Name);
            }
            else
            {
                childParameter = parameter;
            }
            //skip current property and get navigation property expression recursivly
            var innerProperties = properties.Skip(1).ToArray();
            predicate = BuildNavigationExpression(childParameter, comparer, value, innerProperties);
            if (isCollection)
            {
                //build subquery
                resultExpression = BuildSubQuery(parameter, childType, predicate);
            }
            else
            {
                resultExpression = predicate;
            }
        }
        else
        {
            //build final predicate
            resultExpression = BuildCondition(parameter, properties[0], comparer, value);
        }
        return resultExpression;
    }

    private static Expression BuildSubQuery(Expression parameter, Type childType, Expression predicate)
    {
        var anyMethod = typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2);
        anyMethod = anyMethod.MakeGenericMethod(childType);
        predicate = Expression.Call(anyMethod, parameter, predicate);
        return MakeLambda(parameter, predicate);
    }

    private static Expression BuildCondition(Expression parameter, string property, OperatorComparer comparer, object value)
    {
        var childProperty = parameter.Type.GetProperty(property);
        var left          = Expression.Property(parameter, childProperty);
        var right         = Expression.Constant(value);
        var predicate     = BuildComparision(left, comparer, right);
        return MakeLambda(parameter, predicate);
    } 
    
    private static PropertyInfo GetProperty(Type type, string propName)
    {
        PropertyInfo prop = type.GetProperty(propName);

        if (prop == null)
        {
            var csConventionName = propName[0].ToString().ToUpper() + propName.Substring(1);
            prop = type.GetProperty(csConventionName);
        }

        return prop;
    }

    private static Expression BuildComparision(Expression left, OperatorComparer comparer, Expression right)
    {
        var stringSupportedOperators = new List<OperatorComparer>
        {
            OperatorComparer.Equals,
            OperatorComparer.NotEqual,
            OperatorComparer.Contains,
            OperatorComparer.NotContains,
            OperatorComparer.StartsWith,
            OperatorComparer.NotStartsWith,
            OperatorComparer.EndsWith,
            OperatorComparer.NotEndsWith,
            OperatorComparer.In,
            OperatorComparer.NotIn
        };

        if (left.Type != typeof(string))
        {
            //if (comparer == OperatorComparer.In ||
            //    comparer == OperatorComparer.NotIn)
                return BuildCondition(left, comparer, right, left.Type);

            return Expression.MakeBinary((ExpressionType) comparer, left, Expression.Convert(right, left.Type));
        }

        //if (left.Type == typeof(Guid))
        //{
        //    if (!guidSupportedOperators.Contains(comparer))
        //    {
        //        comparer = OperatorComparer.Equals;
        //    }

        //    return BuildGuidCondition(left, comparer, right);
        //}

        return BuildStringCondition(left, comparer, right);
    }

    private static Expression BuildStringCondition(Expression left, OperatorComparer inputComparer, Expression right)
    {
        MethodInfo compareMethod;

        var comparer = Enum.GetName(typeof(OperatorComparer), inputComparer)!
                           .Replace("Not", "")
                           .ToCompareOperator();

        bool needNotOperator = Enum.GetName(typeof(OperatorComparer), inputComparer)!
                                   .StartsWith("Not");

        var toLowerMethod = typeof(string).GetMethods()
                                          .Single(m => m.Name.Equals("ToLower") && !m.GetParameters().Any());

        left = Expression.Call(left, toLowerMethod);

        Expression operatorAppliedExpression;

        if (comparer == OperatorComparer.In)
        {
            compareMethod = typeof(List<string>).GetMethods()
                                                .First(m => m.Name.Equals("Contains") &&
                                                            m.GetParameters().Count() == 1);

            operatorAppliedExpression = Expression.Call(right, compareMethod, left);
        }
        else
        {

            compareMethod = typeof(string).GetMethods()
                                          .First(m => m.Name.Equals(Enum.GetName(typeof(OperatorComparer), comparer)) &&
                                                      m.GetParameters().Count() == 1);

            right = Expression.Call(right, toLowerMethod);

            operatorAppliedExpression = Expression.Call(left, compareMethod, right);
        }

        return needNotOperator ? Expression.Not(operatorAppliedExpression) : operatorAppliedExpression;
    }

    private static Expression BuildCondition(Expression       left,
                                             OperatorComparer inputComparer,
                                             Expression       right,
                                             Type             targetType)
    {
        MethodInfo compareMethod;

        var comparer = Enum.GetName(typeof(OperatorComparer), inputComparer)!
                           .Replace("Not", "")
                           .ToCompareOperator();

        bool needNotOperator = Enum.GetName(typeof(OperatorComparer), inputComparer)!
                                   .StartsWith("Not");

        Expression operatorAppliedExpression;

        if (comparer == OperatorComparer.In)
        {
            compareMethod = typeof(List<>).MakeGenericType(targetType).GetMethod("Contains")!;

            operatorAppliedExpression = Expression.Call(right, compareMethod, left);
        }
        else
        {
            operatorAppliedExpression =
                Expression.MakeBinary((ExpressionType)comparer, left, Expression.Convert(right, left.Type));
        }

        return needNotOperator ? Expression.Not(operatorAppliedExpression) : operatorAppliedExpression;
    }


    private static Expression MakeLambda(Expression parameter, Expression predicate)
    {
        var resultParameterVisitor = new ParameterVisitor();
        resultParameterVisitor.Visit(parameter);
        var resultParameter = resultParameterVisitor.Parameter;
        return Expression.Lambda(predicate, (ParameterExpression)resultParameter);
    }

    private class ParameterVisitor : ExpressionVisitor
    {
        public Expression Parameter
        {
            get;
            private set;
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            Parameter = node;
            return node;
        }
    }
}