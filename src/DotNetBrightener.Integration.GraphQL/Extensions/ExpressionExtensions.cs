using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

internal static class ExpressionExtensions
{
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
            throw new InvalidOperationException($"Unknown field '{fieldName}' of type {type.FullName}");

        var        paramExpression = Expression.Parameter(type, "_");
        Expression memberAccess    = Expression.MakeMemberAccess(paramExpression, propertyInfo);


        if (propertyInfo.PropertyType.IsValueType)
        {
            memberAccess = Expression.Convert(memberAccess, typeof(object));
        }

        var memberAccessExpression = Expression.Lambda<Func<T, object>>(memberAccess, paramExpression);

        return memberAccessExpression;
    }
}