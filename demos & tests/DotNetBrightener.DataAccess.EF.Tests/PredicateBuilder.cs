using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class Entity
{
    public string TargetProp { get; set; }
}

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> BuildInOperatorExpression<T, TSub>(string     propertyPath,
                                                                               List<TSub> arrayOfValues)
    {
        var propertyInfo = typeof(T).GetProperty(propertyPath);

        var parameter = Expression.Parameter(typeof(T), "e");
        var property  = Expression.Property(parameter, propertyInfo);

        var methodInfo = typeof(List<TSub>).GetMethod("Contains") ??
                         throw new InvalidOperationException("Method Contains not found");

        var arrayExpression   = Expression.Constant(arrayOfValues);
        var containsMethodExp = Expression.Call(arrayExpression, methodInfo, property);

        return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameter);
    }
}