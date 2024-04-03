using System.Linq.Expressions;
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
}