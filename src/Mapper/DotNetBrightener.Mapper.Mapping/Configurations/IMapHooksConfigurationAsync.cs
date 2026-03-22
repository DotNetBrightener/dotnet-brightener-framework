using System.Threading;
using System.Threading.Tasks;

namespace DotNetBrightener.Mapper.Mapping.Configurations;

/// <summary>
///     Async version of BeforeMap hook for I/O operations before mapping.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IBeforeMapConfigurationAsync<TSource, TTarget>
{
    /// <summary>
    ///     Called asynchronously before automatic property mapping occurs.
    /// </summary>
    /// <param name="source">
    ///     The source object being mapped from.
    /// </param>
    /// <param name="target">
    ///     The target object being mapped to (properties not yet populated).
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    static abstract Task BeforeMapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}

/// <summary>
///     Async version of AfterMap hook for I/O operations after mapping.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IAfterMapConfigurationAsync<TSource, TTarget>
{
    /// <summary>
    ///     Called asynchronously after automatic property mapping completes.
    /// </summary>
    /// <param name="source">
    ///     The source object that was mapped from.
    /// </param>
    /// <param name="target">
    ///     The target object with all properties now populated.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    static abstract Task AfterMapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}

/// <summary>
///     Combines async BeforeMap and AfterMap hooks in a single interface.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IMapHooksConfigurationAsync<TSource, TTarget>
    : IBeforeMapConfigurationAsync<TSource, TTarget>,
      IAfterMapConfigurationAsync<TSource, TTarget>
{
}

/// <summary>
///     Instance-based async BeforeMap with dependency injection support.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IBeforeMapConfigurationAsyncInstance<TSource, TTarget>
{
    /// <summary>
    ///     Called asynchronously before automatic property mapping occurs.
    /// </summary>
    /// <param name="source">
    ///     The source object being mapped from.
    /// </param>
    /// <param name="target">
    ///     The target object being mapped to (properties not yet populated).
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    Task BeforeMapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}

/// <summary>
///     Instance-based async AfterMap with dependency injection support.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IAfterMapConfigurationAsyncInstance<TSource, TTarget>
{
    /// <summary>
    ///     Called asynchronously after automatic property mapping completes.
    /// </summary>
    /// <param name="source">
    ///     The source object that was mapped from.
    /// </param>
    /// <param name="target">
    ///     The target object with all properties now populated.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    Task AfterMapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}

/// <summary>
///     Instance-based async interface combining BeforeMap and AfterMap hooks with DI support.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IMapHooksConfigurationAsyncInstance<TSource, TTarget>
    : IBeforeMapConfigurationAsyncInstance<TSource, TTarget>,
      IAfterMapConfigurationAsyncInstance<TSource, TTarget>
{
}
