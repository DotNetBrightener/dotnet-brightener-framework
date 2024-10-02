using System.Collections.Immutable;
using System.Linq.Expressions;
using DotNetBrightener.DataAccess.Models.Auditing;
using Microsoft.EntityFrameworkCore.Query;

namespace DotNetBrightener.DataAccess.EF.Extensions;

public class SetPropertyBuilder<TSource>
{
    public Expression<Func<SetPropertyCalls<TSource>, SetPropertyCalls<TSource>>>
        SetPropertyCalls { get; private set; } = b => b;

    /// <summary>
    ///     Executes setting given value to the property from the given expression
    /// </summary>
    /// <typeparam name="TProperty">The type of the property</typeparam>
    /// <param name="propertyExpression">The expression of how to access the property</param>
    /// <param name="value">The value to set</param>
    /// <returns>
    ///     The same instance so that multiple calls to
    ///     <see cref="SetPropertyBuilder{TSource}.SetProperty{TProperty}(Expression{Func{TSource, TProperty}}, TProperty)" />
    ///     can be chained.
    /// </returns>
    public SetPropertyBuilder<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression,
                                                              TProperty                            value) =>
        SetProperty(propertyExpression, _ => value);

    /// <summary>
    ///     Executes setting given value to the property from the given expression
    /// </summary>
    /// <typeparam name="TProperty">The type of the property</typeparam>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="value">The value to set</param>
    /// <returns>
    ///     The same instance so that multiple calls to
    ///     <see cref="SetPropertyByName{TProperty}" />
    ///     can be chained.
    /// </returns>
    public SetPropertyBuilder<TSource> SetPropertyByName<TProperty>(string    propertyName,
                                                                    TProperty value) =>
        SetProperty(GetPropertyExpression<TProperty>(propertyName), _ => value);


    /// <summary>
    ///     Executes setting given value to the property from the given expression
    /// </summary>
    /// <typeparam name="TProperty">The type of the property</typeparam>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="valueExpression">Expression on how to set the property</param>
    /// <returns>
    ///     The same instance so that multiple calls to
    ///     <see cref="SetPropertyByNameAndExpression{TProperty}" />
    ///     can be chained.
    /// </returns>
    public SetPropertyBuilder<TSource> SetPropertyByNameAndExpression<TProperty>(string propertyName,
                                                                                 Expression<Func<TSource, TProperty>>
                                                                                     valueExpression) =>
        SetProperty(GetPropertyExpression<TProperty>(propertyName), valueExpression);

    private SetPropertyBuilder<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression,
                                                               Expression<Func<TSource, TProperty>> valueExpression)
    { 
        var memberExpression = propertyExpression.Body as MemberExpression;
        if (propertyExpression.Body is UnaryExpression unaryExpression)
        {
            memberExpression = unaryExpression.Operand as MemberExpression;
        }

        if (memberExpression != null)
        {
            var compiledValueExpression = valueExpression.Compile();
            var newValue = compiledValueExpression.DynamicInvoke(Activator.CreateInstance(typeof(TSource)));

            string changeDescription = GetChangeDescription(valueExpression.Body);

            if (!string.IsNullOrEmpty(changeDescription) && !changeDescription.Equals("value"))
            {
                newValue = changeDescription;
            }

            AuditProperties.Add(new AuditProperty
            {
                PropertyName = memberExpression.Member.Name,
                OldValue     = "Not Tracked", // OldValue isn't available with this method
                NewValue     = newValue
            });
        }

        SetPropertyCalls = SetPropertyCalls.Update(
                                                   body: Expression.Call(
                                                                         instance: SetPropertyCalls.Body,
                                                                         methodName: nameof(SetPropertyCalls<TSource>
                                                                                               .SetProperty),
                                                                         typeArguments: new[]
                                                                         {
                                                                             typeof(TProperty)
                                                                         },
                                                                         arguments: new Expression[]
                                                                         {
                                                                             propertyExpression, valueExpression
                                                                         }
                                                                        ),
                                                   parameters: SetPropertyCalls.Parameters
                                                  );

        return this;
    }

    private static Expression<Func<TSource, TProperty>> GetPropertyExpression<TProperty>(string propertyName)
    {
        ParameterExpression parameter          = Expression.Parameter(typeof(TSource), "source");
        MemberExpression    property           = Expression.Property(parameter, propertyName);
        UnaryExpression     propertyConversion = Expression.Convert(property, typeof(TProperty));

        var propertyLambda = Expression.Lambda<Func<TSource, TProperty>>(propertyConversion, parameter);

        return propertyLambda;
    }

    public List<AuditProperty> AuditProperties { get; } = new List<AuditProperty>();

    public ImmutableList<AuditProperty> ExtractAuditProperties()
    {
        return AuditProperties.ToImmutableList();
    }

    private string GetChangeDescription(Expression expression)
    {
        switch (expression)
        {
            case BinaryExpression binaryExpression:
                var left  = GetChangeDescription(binaryExpression.Left);
                var right = GetChangeDescription(binaryExpression.Right);

                return $"origin.{left} {binaryExpression.NodeType.ToString()} {right}";

            case MemberExpression memberExpression:
                // Check if the expression is accessing a property of the original entity
                return memberExpression.Member.Name;

            case ConstantExpression constantExpression:
                // Return the constant value as a string
                return constantExpression.Value?.ToString();

            default:
                return string.Empty;
        }
    }
}