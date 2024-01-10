using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    private static Expression<Func<TIn, bool>> BuildBooleanPredicateQuery<TIn>(string           filterValue,
                                                                               PropertyPathInfo property)
        where TIn : class
    {
        OperatorComparer operation = OperatorComparer.Equals;

        var value = string.IsNullOrEmpty(filterValue) || (bool.TryParse(filterValue, out var b)
                                                              ? b
                                                              : throw new
                                                                    InvalidOperationException("Boolean value must be True / False or left empty"));

        var predicateQuery = ExpressionExtensions.BuildPredicate<TIn>(value,
                                                                      operation,
                                                                      property.Path);

        return predicateQuery;
    }
}