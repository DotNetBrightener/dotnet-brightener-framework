using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public partial class QueryableDeepFilterExtensions
{
    /// <summary>
    ///     Returns the property access name from the given selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    public static string PickProperty<TIn>(this Expression<Func<TIn, object>> selector)
    {
        return selector.Body switch
        {
            MemberExpression mae                                    => mae.Member.Name,
            UnaryExpression { Operand: MemberExpression subSelect } => subSelect.Member.Name,
            _                                                       => ""
        };
    }

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    public static string PickProperty<TIn, TNext>(this Expression<Func<TIn, IEnumerable<TNext>>> selector,
                                                  Expression<Func<TNext, object>>                subSelector)
    {
        string initProp = selector.Body switch
        {
            MemberExpression mae => mae.Member.Name,
            UnaryExpression { Operand: MemberExpression mainSelect } =>
                mainSelect.Member.Name,
            _ => ""
        };

        if (string.IsNullOrEmpty(initProp))
            return "";

        return subSelector.Body switch
        {
            MemberExpression subSelectorBody => initProp + "." + subSelectorBody.Member.Name,
            UnaryExpression { Operand: MemberExpression subSelect } => initProp + "." +
                                                                       subSelect.Member.Name,
            _ => ""
        };
    }

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    public static string PickProperty<TIn, TNext>(this Expression<Func<TIn, TNext>> selector,
                                                  Expression<Func<TNext, object>>   subSelector)
    {
        string initProp = selector.Body switch
        {
            MemberExpression mae => mae.Member.Name,
            UnaryExpression { Operand: MemberExpression mainSelect } =>
                mainSelect.Member.Name,
            _ => ""
        };

        if (string.IsNullOrEmpty(initProp))
            return "";

        return subSelector.Body switch
        {
            MemberExpression subSelectorBody => initProp + "." + subSelectorBody.Member.Name,
            UnaryExpression { Operand: MemberExpression subSelect } => initProp + "." +
                                                                       subSelect.Member.Name,
            _ => ""
        };
    }
}