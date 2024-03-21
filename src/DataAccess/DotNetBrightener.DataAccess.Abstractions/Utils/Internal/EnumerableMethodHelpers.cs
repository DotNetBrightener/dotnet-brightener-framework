using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetBrightener.DataAccess.Utils.Internal;

internal static class EnumerableMethodHelpers
{
    /// <summary>
    ///     Retrieves the generic <see cref="Enumerable.Select{TSource,TResult}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TResult})"/> method;
    /// </summary>
    internal static readonly MethodInfo NestedSelectMethod = (from x in typeof(Enumerable).GetMethods()
                                                              where x.Name == "Select" && x.IsGenericMethod
                                                              let gens = x.GetGenericArguments()
                                                              where gens.Length == 2
                                                              let pars = x.GetParameters()
                                                              where pars.Length == 2 &&
                                                                    pars [0].ParameterType ==
                                                                    typeof(IEnumerable<>)
                                                                       .MakeGenericType(gens [0]) &&
                                                                    pars [1].ParameterType ==
                                                                    typeof(Func<,>).MakeGenericType(gens)
                                                              select x).Single();

    /// <summary>
    ///     Retrieves the generic <see cref="Expression.Lambda{TDelegate}(System.Linq.Expressions.Expression,System.Collections.Generic.IEnumerable{System.Linq.Expressions.ParameterExpression}?)"/>
    /// </summary>
    internal static readonly MethodInfo LambdaMethod = (from x in typeof(Expression).GetMethods()
                                                        where x.Name == "Lambda" && x.IsGenericMethod
                                                        let parameters = x.GetParameters()
                                                        where parameters.Length == 2 &&
                                                              parameters [0].ParameterType == typeof(Expression) &&
                                                              parameters [1].ParameterType ==
                                                              typeof(ParameterExpression [ ])
                                                        select x).Single();
}