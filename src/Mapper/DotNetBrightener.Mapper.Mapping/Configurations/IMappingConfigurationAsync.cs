using System.Threading;
using System.Threading.Tasks;

namespace DotNetBrightener.Mapper.Mapping.Configurations;

/// <summary>
///     Allows defining async custom mapping logic between a source and generated target type.
///     Use this interface when your mapping logic requires async operations like database calls, API calls, or I/O operations.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IMappingConfigurationAsync<TSource, TTarget>
{
    /// <summary>
    ///     Asynchronously maps source to target with custom logic.
    ///     This method is called after the standard property copying is completed.
    /// </summary>
    /// <param name="source">
    ///     The source object
    /// </param>
    /// <param name="target">
    ///     The target object with basic properties already copied
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token to cancel the async operation
    /// </param>
    /// <returns>
    ///     A task representing the async mapping operation
    /// </returns>
    static abstract Task MapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}

/// <summary>
///     Instance-based interface for defining async custom mapping logic with dependency injection support.
///     Use this interface when your async mapping logic requires injected services.
/// </summary>
/// <typeparam name="TSource">
///     The source type
/// </typeparam>
/// <typeparam name="TTarget">
///     The target type
/// </typeparam>
public interface IMappingConfigurationAsyncInstance<TSource, TTarget>
{
    /// <summary>
    ///     Asynchronously maps source to target with custom logic.
    ///     This method is called after the standard property copying is completed.
    /// </summary>
    /// <param name="source">
    ///     The source object
    /// </param>
    /// <param name="target">
    ///     The target object with basic properties already copied
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token to cancel the async operation
    /// </param>
    /// <returns>
    ///     A task representing the async mapping operation
    /// </returns>
    Task MapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}
