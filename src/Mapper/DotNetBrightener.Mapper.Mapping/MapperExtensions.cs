using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotNetBrightener.Mapper;

namespace DotNetBrightener.Mapper.Mapping;

/// <summary>
///     Provides extension methods for mapping source entities or sequences
///     to generated target types (synchronous and provider-agnostic only).
/// </summary>
public static class MapperExtensions
{
    // For a target type TTarget, cache the declared source type from [MappingTarget<TSource>].
    private static readonly ConcurrentDictionary<Type, Type> _declaredSourceTypeByTarget = new();

    // Cached MethodInfo for ToTarget<TSource, TTarget>(TSource)
    private static readonly MethodInfo _toTargetTwoGenericMethod =
        typeof(MapperExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m =>
            {
                if (m.Name != nameof(ToTarget)) return false;
                var ga = m.GetGenericArguments();
                if (ga.Length != 2) return false;
                var ps = m.GetParameters();
                return ps.Length == 1;
            });

    // Cached MethodInfo for ToSource<TTarget, TSource>(TTarget)
    private static readonly MethodInfo _toSourceTwoGenericMethod =
        typeof(MapperExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m =>
            {
                if (m.Name != nameof(ToSource)) return false;
                var ga = m.GetGenericArguments();
                if (ga.Length != 2) return false;
                var ps = m.GetParameters();
                return ps.Length == 1;
            });

    // Cached Expression<Func<DeclaredSource, TTarget>> from TTarget.Projection.
    private static readonly ConcurrentDictionary<Type, LambdaExpression> _declaredProjectionByTarget = new();

    // Cache of adapted Expression<Func<TElement, TTarget>> shapes per (element, target).
    private static readonly ConcurrentDictionary<(Type ElementType, Type TargetType), LambdaExpression>
        _adaptedProjectionByElementAndTarget = new();

    /// <summary>
    ///     Maps a single source instance to the specified target type by invoking its generated constructor.
    ///     If the constructor fails (e.g., due to required init-only properties), attempts to use a static FromSource factory method.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type.
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type, which must have a public constructor accepting <c>TSource</c> or a static FromSource method.
    /// </typeparam>
    /// <param name="source">
    ///     The source instance to map.
    /// </param>
    /// <returns>
    ///     A new <typeparamref name="TTarget"/> instance populated from <paramref name="source"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public static TTarget ToTarget<TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] TTarget>(this TSource source)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));    
        return MappingCache<TSource, TTarget>.Mapper(source);
    }

    /// <summary>
    ///     Converts the specified source object to an instance of the target type annotated as a target.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The target type to which the source object will be converted. Must be a reference type and annotated with
    ///     <c>[MappingTarget&lt;TSource&gt;]</c>.
    /// </typeparam>
    /// <param name="source">
    ///     The source object to be converted. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     An instance of the target type <typeparamref name="TTarget"/> created from the source object.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 The target type <typeparamref name="TTarget"/> is not
    ///                 annotated with <c>[MappingTarget&lt;TSource&gt;]</c>.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The source object's type is
    ///                 not assignable to the declared source type for the target.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The conversion process fails due to a missing constructor or static <c>FromSource</c> method.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    [RequiresUnreferencedCode("This method uses MakeGenericMethod which is not compatible with trimming. Use the strongly-typed ToTarget<TSource, TTarget> overload instead.")]
    [RequiresDynamicCode("This method uses MakeGenericMethod which requires dynamic code generation. Use the strongly-typed ToTarget<TSource, TTarget> overload instead.")]
    public static TTarget ToTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] TTarget>(this object source)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var targetType = typeof(TTarget);

        var declaredSource = GetDeclaredSourceType(targetType)
            ?? throw new InvalidOperationException(
                $"Type '{targetType.FullName}' must be annotated with [MappingTarget<TSource>] to use ToTarget<{targetType.Name}>().");

        if (!declaredSource.IsInstanceOfType(source))
        {
            throw new InvalidOperationException(
                $"Source instance type '{source.GetType().FullName}' is not assignable to declared MappingTarget source '{declaredSource.FullName}' for target '{targetType.FullName}'.");
        }

        var forwarded = _toTargetTwoGenericMethod.MakeGenericMethod(declaredSource, targetType)
                                         .Invoke(null, new[] { source });
        if (forwarded is null)
        {
            throw new InvalidOperationException(
                $"Unable to map source '{declaredSource.FullName}' to '{targetType.FullName}'. Ensure a matching constructor or static FromSource exists.");
        }

        return (TTarget)forwarded;
    }

    /// <summary>
    ///     Maps a single target instance to the specified source type by invoking its generated ToSource method.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The target type that is annotated with [MappingTarget&lt;TSource&gt;].
    /// </typeparam>
    /// <typeparam name="TSource">
    ///     The entity type to map to.
    /// </typeparam>
    /// <param name="target">
    ///     The target instance to map.
    /// </param>
    /// <returns>
    ///     A new <typeparamref name="TSource"/> instance populated from <paramref name="target"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="target"/> is <c>null</c>.
    /// </exception>
    public static TSource ToSource<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TTarget, TSource>(this TTarget target)
        where TTarget : class
        where TSource : class
    {
        if (target is null) throw new ArgumentNullException(nameof(target));
        return MappingSourceCache<TTarget, TSource>.Mapper(target);
    }

    /// <summary>
    ///     Converts the specified target object to an instance of the source type that the target was created from.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source type to which the target object will be converted. Must be a reference type.
    /// </typeparam>
    /// <param name="target">
    ///     The target object to be converted. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     An instance of the source type <typeparamref name="TSource"/> created from the target object.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 The target type is not
    ///                 annotated with <c>[MappingTarget&lt;TSource&gt;]</c>.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The target type does not match
    ///                 the declared source type for the target.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The conversion process fails due to a missing ToSource method on the target.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    [RequiresUnreferencedCode("This method uses MakeGenericMethod which is not compatible with trimming. Use the strongly-typed ToSource<TTarget, TSource> overload instead.")]
    [RequiresDynamicCode("This method uses MakeGenericMethod which requires dynamic code generation. Use the strongly-typed ToSource<TTarget, TSource> overload instead.")]
    public static TSource ToSource<TSource>(this object target)
        where TSource : class
    {
        if (target is null) throw new ArgumentNullException(nameof(target));

        var targetType = target.GetType();
        var declaredSource = GetDeclaredSourceType(targetType)
            ?? throw new InvalidOperationException(
                $"Type '{targetType.FullName}' must be annotated with [MappingTarget<TSource>] to use ToSource<{typeof(TSource).Name}>().");

        if (declaredSource != typeof(TSource))
        {
            throw new InvalidOperationException(
                $"Target type '{typeof(TSource).FullName}' does not match declared MappingTarget source '{declaredSource.FullName}' for target '{targetType.FullName}'.");
        }

        var forwarded = _toSourceTwoGenericMethod.MakeGenericMethod(targetType, typeof(TSource))
                                         .Invoke(null, new[] { target });
        if (forwarded is null)
        {
            throw new InvalidOperationException(
                $"Unable to map target '{targetType.FullName}' to '{typeof(TSource).FullName}'. Ensure the target has a generated ToSource method.");
        }

        return (TSource)forwarded;
    }

    /// <summary>
    ///     Maps an <see cref="IEnumerable{TSource}"/> to an <see cref="IEnumerable{TTarget}"/>
    ///     via the generated constructor of the target type.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type.
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type, which must have a public constructor accepting <c>TSource</c>.
    /// </typeparam>
    /// <param name="source">
    ///     The enumerable source of entities.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{TTarget}"/> containing mapped target instances.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public static IEnumerable<TTarget> SelectTargets<TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] TTarget>(this IEnumerable<TSource> source)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var mapper = MappingCache<TSource, TTarget>.Mapper;

        if (source is ICollection<TSource> collection)
        {
            var result = new List<TTarget>(collection.Count);
            foreach (var item in source)
            {
                result.Add(mapper(item));
            }
            return result;
        }

        var list = new List<TTarget>();
        foreach (var item in source)
        {
            list.Add(mapper(item));
        }
        return list;
    }

    /// <summary>
    ///     Maps an <see cref="IEnumerable{TTarget}"/> to an <see cref="IEnumerable{TSource}"/>
    ///     via the generated ToSource method of the target type.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The target type, which must be annotated with [MappingTarget&lt;TSource&gt;].
    /// </typeparam>
    /// <typeparam name="TSource">
    ///     The source type.
    /// </typeparam>
    /// <param name="targets">
    ///     The source collection of targets.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{TSource}"/> mapped from the input.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="targets"/> is <c>null</c>.
    /// </exception>
    public static IEnumerable<TSource> SelectSources<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TTarget, TSource>(this IEnumerable<TTarget> targets)
        where TTarget : class
        where TSource : class
    {
        if (targets is null) throw new ArgumentNullException(nameof(targets));

        var mapper = MappingSourceCache<TTarget, TSource>.Mapper;

        if (targets is ICollection<TTarget> collection)
        {
            var result = new List<TSource>(collection.Count);
            foreach (var target in targets)
            {
                result.Add(mapper(target));
            }
            return result;
        }

        var list = new List<TSource>();
        foreach (var target in targets)
        {
            list.Add(mapper(target));
        }
        return list;
    }
    
    /// <summary>
    ///     Maps an <see cref="IEnumerable"/> of target objects to an <see cref="IEnumerable{TSource}"/>
    ///     via the generated ToSource method of each target type.
    /// </summary>
    /// <remarks>
    ///     This method lazily converts each non-null target object by calling <see cref="ToSource{TSource}(object)"/> on each element.
    ///     Only non-null elements are processed; nulls are skipped. The operation uses deferred execution and
    ///     preserves the order of the source sequence.
    ///     <para>
    ///         Note: Each target object must be annotated with <c>[MappingTarget&lt;TSource&gt;]</c> and have a generated ToSource method.
    ///         If a target type is not properly annotated or lacks the required ToSource method, the underlying
    ///         <see cref="ToSource{TSource}(object)"/>
    ///         may throw <see cref="InvalidOperationException"/> at iteration time.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TSource">
    ///     The source type to map back to. Must be a reference type.
    /// </typeparam>
    /// <param name="targets">
    ///     The source collection of target objects. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{TSource}"/> containing source instances mapped from the non-null target objects in the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="targets"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown at iteration time if any target object is not annotated with <c>[MappingTarget&lt;TSource&gt;]</c>, or lacks a generated ToSource method.
    /// </exception>
    [RequiresUnreferencedCode("This method uses MakeGenericMethod which is not compatible with trimming. Use the strongly-typed SelectSources<TTarget, TSource> overload instead.")]
    [RequiresDynamicCode("This method uses MakeGenericMethod which requires dynamic code generation. Use the strongly-typed SelectSources<TTarget, TSource> overload instead.")]
    public static IEnumerable<TSource> SelectSources<TSource>(this IEnumerable targets)
        where TSource : class
    {
        if (targets is null) throw new ArgumentNullException(nameof(targets));
        foreach (var item in targets)
        {
            if (item is null) continue;
            yield return item.ToSource<TSource>();
        }
    }

    /// <summary>
    ///     Projects each non-null element of the source sequence into <typeparamref name="TTarget"/>.
    /// </summary>
    /// <remarks>
    ///     This method lazily converts items by calling <see cref="ToTarget{TTarget}"/> on each element.
    ///     Only non-null elements are processed; nulls are skipped. The operation uses deferred execution and
    ///     preserves the order of the source sequence.
    ///     <para>
    ///         Note: <typeparamref name="TTarget"/> must be a Mapper-generated type (annotated with <c>[MappingTarget]</c>).
    ///         If the target type is not annotated or lacks a matching constructor/factory, the underlying
    ///         <see cref="ToTarget{TTarget}"/>
    ///         may throw <see cref="InvalidOperationException"/> at iteration time.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TTarget">
    ///     The target type to project to (reference type).
    /// </typeparam>
    /// <param name="source">
    ///     The source sequence. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}"/> of <typeparamref name="TTarget"/> created from the non-null elements
    ///     of <paramref name="source"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    [RequiresUnreferencedCode("This method uses MakeGenericMethod which is not compatible with trimming. Use the strongly-typed SelectTargets<TSource, TTarget> overload instead.")]
    [RequiresDynamicCode("This method uses MakeGenericMethod which requires dynamic code generation. Use the strongly-typed SelectTargets<TSource, TTarget> overload instead.")]
    public static IEnumerable<TTarget> SelectTargets<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] TTarget>(this IEnumerable source)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        foreach (var item in source)
        {
            if (item is null) continue;
            yield return item.ToTarget<TTarget>();
        }
    }

    /// <summary>
    ///     Projects an <see cref="IQueryable{TSource}"/> to an <see cref="IQueryable{TTarget}"/>
    ///     using the static <c>Expression&lt;Func&lt;TSource,TTarget&gt;&gt;</c> named <c>Projection</c> defined on <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type.
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type, which must define a public static <c>Expression&lt;Func&lt;TSource,TTarget&gt;&gt; Projection</c>.
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source of entities.
    /// </param>
    /// <returns>
    ///     An <see cref="IQueryable{TTarget}"/> representing the projection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="source"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <typeparamref name="TTarget"/> does not define a static <c>Projection</c> property.
    /// </exception>
    public static IQueryable<TTarget> SelectTarget<TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>(this IQueryable<TSource> source)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var prop = typeof(TTarget).GetProperty(
            "Projection",
            BindingFlags.Public | BindingFlags.Static);

        if (prop is null)
            throw new InvalidOperationException(
                $"Type {typeof(TTarget).Name} must define a public static Projection property.");

        var expr = (Expression<Func<TSource, TTarget>>)prop.GetValue(null)!;
        return source.Select(expr);
    }

    /// <summary>
    ///     Projects the elements of the source query into <typeparamref name="TTarget"/> using the target's generated projection.
    /// </summary>
    /// <remarks>
    ///     Uses <c>TTarget.Projection</c> (an <see cref="Expression{TDelegate}"/> of type
    ///     <c>
    ///     Expression&lt;Func&lt;DeclaredSource, TTarget&gt;&gt;</c>) and adapts the parameter to the query's
    ///     element type if necessary (by inserting a cast). This builds an expression tree only (no materialization)
    ///     and therefore uses deferred execution; translation behavior is provider-dependent.
    /// </remarks>
    /// <typeparam name="TTarget">
    ///     The target type (class) annotated with <c>[MappingTarget]</c> and exposing a public static <c>Projection</c> property.
    /// </typeparam>
    /// <param name="source">
    ///     The source <see cref="IQueryable"/>. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     An <see cref="IQueryable{T}"/> of <typeparamref name="TTarget"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if <typeparamref name="TTarget"/> is not annotated with a <c>[MappingTarget]</c> attribute or does not define a
    ///     static <c>Projection</c> property.
    /// </exception>
    public static IQueryable<TTarget> SelectTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>(this IQueryable source)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var targetType = typeof(TTarget);        

        var declaredProjection = GetDeclaredProjectionLambda(targetType);

        // Adapt the declared projection to the source's actual element type, if needed.
        var adapted = GetOrBuildAdaptedProjection(source.ElementType, targetType, declaredProjection);

        // Build Queryable.Select<TElement, TTarget>(source.Expression, adapted)
        var selectCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Select),
            new[] { source.ElementType, targetType },
            source.Expression,
            adapted);

        return source.Provider.CreateQuery<TTarget>(selectCall);
    }

    private static Type? GetDeclaredSourceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type targetType)
    {
        if (_declaredSourceTypeByTarget.TryGetValue(targetType, out var cached))
            return cached;

        var attr = targetType
            .GetCustomAttributesData()
            .FirstOrDefault(a =>
            {
                var attributeType = a.AttributeType;
                return attributeType.IsGenericType &&
                       attributeType.GetGenericTypeDefinition() == typeof(MappingTargetAttribute<>);
            });

        Type? declared = null;
        if (attr is not null)
        {
            declared = attr.AttributeType.GenericTypeArguments[0];
        }

        if (declared != null)
        {
            _declaredSourceTypeByTarget[targetType] = declared;
        }

        return declared;
    }

    private static LambdaExpression GetDeclaredProjectionLambda([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type targetType)
    {
        if (_declaredProjectionByTarget.TryGetValue(targetType, out var cached))
            return cached;

        var prop = targetType.GetProperty("Projection", BindingFlags.Public | BindingFlags.Static)
                  ?? throw new InvalidOperationException(
                      $"Type {targetType.Name} must define a public static Projection property.");

        var value = prop.GetValue(null)
                   ?? throw new InvalidOperationException($"{targetType.Name}.Projection returned null.");

        if (value is not LambdaExpression lambda)
            throw new InvalidOperationException($"{targetType.Name}.Projection must be an Expression<Func<..., {targetType.Name}>>.");
        
        _declaredProjectionByTarget[targetType] = lambda;
        return lambda;
    }

    private static LambdaExpression GetOrBuildAdaptedProjection(Type elementType, Type targetType, LambdaExpression declaredProjection)
    {
        var key = (elementType, targetType);
        if (_adaptedProjectionByElementAndTarget.TryGetValue(key, out var cached))
            return cached;

        // If element type matches the projection's parameter type, use it as-is.
        var declaredParam = declaredProjection.Parameters[0];
        if (declaredParam.Type == elementType)
        {
            _adaptedProjectionByElementAndTarget[key] = declaredProjection;
            return declaredProjection;
        }

        // Otherwise, rebuild: (TElement e) => [declaredProjection.Body with param replaced by (DeclaredSource)e]
        var newParam = Expression.Parameter(elementType, declaredParam.Name);
        var replacement = Expression.Convert(newParam, declaredParam.Type); // cast to declared source
        var body = new ReplaceParameterVisitor(declaredParam, replacement).Visit(declaredProjection.Body)
                   ?? throw new InvalidOperationException("Failed to adapt Projection expression.");

        var adapted = Expression.Lambda(body, newParam);
        _adaptedProjectionByElementAndTarget[key] = adapted;
        return adapted;
    }

    /// <summary>
    ///     Applies changed properties from a target DTO back to the source object.
    ///     Only updates properties that exist in both the target and the source and have different values.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type being updated
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target DTO type containing the new values
    /// </typeparam>
    /// <param name="source">
    ///     The source instance to update
    /// </param>
    /// <param name="target">
    ///     The target DTO containing the new property values
    /// </param>
    /// <returns>
    ///     The updated source instance (for fluent chaining)
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source or target is null
    /// </exception>
    /// <example>
    ///     <code>
    ///         var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    ///         var target = new UserDto { Name = "Jane", Email = "john@example.com" };
    ///         user.ApplyTarget&lt;User, UserDto&gt;(target);
    ///         // user.Name is now "Jane", Email unchanged
    ///     </code>
    /// </example>
    public static TSource ApplyTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>(this TSource source, TTarget target)
        where TSource : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));

        // Get properties that exist in both target and source
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p);

        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        // Update source properties from target where values differ
        foreach (var sourceProperty in sourceProperties)
        {
            if (targetProperties.TryGetValue(sourceProperty.Name, out var targetProperty))
            {
                var targetValue = targetProperty.GetValue(target);
                var sourceValue = sourceProperty.GetValue(source);

                // Only update if values are different
                if (!Equals(targetValue, sourceValue))
                {
                    sourceProperty.SetValue(source, targetValue);
                }
            }
        }

        return source;
    }

    /// <summary>
    ///     Applies changed properties from a target DTO back to the source object.
    ///     The target type is inferred from the TTarget parameter.
    ///     Only updates properties that exist in both the target and the source and have different values.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The target DTO type containing the new values
    /// </typeparam>
    /// <param name="source">
    ///     The source instance to update
    /// </param>
    /// <param name="target">
    ///     The target DTO containing the new property values
    /// </param>
    /// <returns>
    ///     The updated source instance (for fluent chaining)
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source or target is null
    /// </exception>
    /// <example>
    ///     <code>
    ///         var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    ///         var target = new UserDto { Name = "Jane", Email = "john@example.com" };
    ///         user.ApplyTarget(target);
    ///         // user.Name is now "Jane", Email unchanged
    ///     </code>
    /// </example>
    [RequiresUnreferencedCode("This method uses reflection on the runtime type of the source object. Use the strongly-typed ApplyTarget<TSource, TTarget> overload instead.")]
    public static object ApplyTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>(this object source, TTarget target)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));

        var sourceType = source.GetType();

        // Get properties that exist in both target and source
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p);

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        // Update source properties from target where values differ
        foreach (var sourceProperty in sourceProperties)
        {
            if (targetProperties.TryGetValue(sourceProperty.Name, out var targetProperty))
            {
                var targetValue = targetProperty.GetValue(target);
                var sourceValue = sourceProperty.GetValue(source);

                // Only update if values are different
                if (!Equals(targetValue, sourceValue))
                {
                    sourceProperty.SetValue(source, targetValue);
                }
            }
        }

        return source;
    }

    /// <summary>
    ///     Applies changed properties from a target DTO back to the source object and returns information about which properties were changed.
    ///     This is useful for auditing, logging, or conditional logic based on what actually changed.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type being updated
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target DTO type containing the new values
    /// </typeparam>
    /// <param name="source">
    ///     The source instance to update
    /// </param>
    /// <param name="target">
    ///     The target DTO containing the new property values
    /// </param>
    /// <returns>
    ///     A result containing the updated source and a list of property names that were changed
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source or target is null
    /// </exception>
    /// <example>
    ///     <code>
    ///         var result = user.ApplyTargetWithChanges&lt;User, UserDto&gt;(target);
    ///         if (result.HasChanges)
    ///         {
    ///             Console.WriteLine($"Changed properties: {string.Join(", ", result.ChangedProperties)}");
    ///         }
    ///     </code>
    /// </example>
    public static TargetApplyResult<TSource> ApplyTargetWithChanges<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>(this TSource source, TTarget target)
        where TSource : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));

        var changedProperties = new List<string>();

        // Get properties that exist in both target and source
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p);

        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        // Update source properties from target where values differ
        foreach (var sourceProperty in sourceProperties)
        {
            if (targetProperties.TryGetValue(sourceProperty.Name, out var targetProperty))
            {
                var targetValue = targetProperty.GetValue(target);
                var sourceValue = sourceProperty.GetValue(source);

                // Only update if values are different
                if (!Equals(targetValue, sourceValue))
                {
                    sourceProperty.SetValue(source, targetValue);
                    changedProperties.Add(sourceProperty.Name);
                }
            }
        }

        return new TargetApplyResult<TSource>(source, changedProperties);
    }

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly Expression _newExpr;
        public ReplaceParameterVisitor(ParameterExpression oldParam, Expression newExpr)
        {
            _oldParam = oldParam ?? throw new ArgumentNullException(nameof(oldParam));
            _newExpr = newExpr ?? throw new ArgumentNullException(nameof(newExpr));
        }
        protected override Expression VisitParameter(ParameterExpression node)
            => node == _oldParam ? _newExpr : base.VisitParameter(node);
    }
}

/// <summary>
///     Represents the result of a target apply operation, containing the updated source and information about what changed.
/// </summary>
/// <typeparam name="TSource">
///     The type of source that was updated
/// </typeparam>
public readonly record struct TargetApplyResult<TSource>(
    TSource Source,
    IReadOnlyList<string> ChangedProperties)
    where TSource : class
{
    /// <summary>
    ///     Gets a value indicating whether any properties were changed during the apply operation.
    /// </summary>
    public bool HasChanges => ChangedProperties.Count > 0;
}
