using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetBrightener.Mapper.Mapping;

/// <summary>
///     Provides a cached <see cref="Func{TTarget, TSource}"/> mapping delegate used by
///     <c>
///         ToSource&lt;TTarget, TSource&gt;</c> to efficiently construct <typeparamref name="TSource"/> instances
///         from <typeparamref name="TTarget"/> values.
///     </summary>
///     <typeparam name="TTarget">
///         The mapping target type that is annotated with [MappingTarget&lt;TSource&gt;].
///     </typeparam>
///     <typeparam name="TSource">
///         The target entity type. Must expose either a public static <c>FromTarget(<typeparamref name="TTarget"/>)
///     </c>
///     factory method, or a public constructor accepting a <typeparamref name="TTarget"/> instance.
/// </typeparam>
/// <remarks>
///     This type performs reflection only once per <typeparamref name="TTarget"/> / <typeparamref name="TSource"/>
///     combination, precompiling a delegate for reuse in all subsequent mappings.
/// </remarks>
/// <exception cref="InvalidOperationException">
///     Thrown when no usable <c>FromTarget</c> factory or compatible constructor is found on <typeparamref name="TSource"/>.
/// </exception>
internal static class MappingSourceCache<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
    TTarget, TSource>
    where TTarget : class
    where TSource : class
{
    public static readonly Func<TTarget, TSource> Mapper = CreateMapper();

    private static Func<TTarget, TSource> CreateMapper()
    {
        // Look for the ToSource() method first (new name), then BackTo() for backwards compatibility
        var toEntityMethod = typeof(TTarget).GetMethod(
                                                       "ToSource",
                                                       BindingFlags.Public | BindingFlags.Instance,
                                                       null,
                                                       Type.EmptyTypes,
                                                       null);
        
        if (toEntityMethod != null &&
            toEntityMethod.ReturnType == typeof(TSource))
        {
            // Use compiled expression instead of DynamicMethod for AOT/trimming compatibility
            var param    = Expression.Parameter(typeof(TTarget), "target");
            var callExpr = Expression.Call(param, toEntityMethod);
            var lambda   = Expression.Lambda<Func<TTarget, TSource>>(callExpr, param);

            return lambda.Compile();
        }

        // If no ToSource/BackTo method is found, provide a helpful error message
        throw new InvalidOperationException(
                                            $"Unable to map {typeof(TTarget).Name} to {typeof(TSource).Name}: " +
                                            $"no ToSource() method found on the target type. Ensure the target is properly generated with source generation.");
    }
}
