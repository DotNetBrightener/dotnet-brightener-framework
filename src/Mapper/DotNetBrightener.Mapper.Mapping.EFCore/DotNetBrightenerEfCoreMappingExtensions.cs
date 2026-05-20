using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetBrightener.Mapper.Mapping.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Mapper.Mapping.EFCore;

/// <summary>
///     EF Core extension methods for target mapping with custom async mapper support.
///     These methods enable complex mappings that cannot be expressed as SQL projections.
/// </summary>
public static class DotNetBrightenerEfCoreMappingExtensions
{
    /// <summary>
    ///     Asynchronously projects an IQueryable&lt;TSource&gt; to a List&lt;TTarget&gt;
    ///     using a custom async mapper instance. This is useful for complex mappings that require
    ///     dependency injection or cannot be expressed as SQL projections.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source
    /// </param>
    /// <param name="mapper">
    ///     The async mapper instance (supports dependency injection)
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token
    /// </param>
    /// <returns>
    ///     A list of mapped target instances
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source or mapper is null
    /// </exception>
    /// <remarks>
    ///     This method first executes the query to materialize the source entities,
    ///     then applies the custom mapper to each result. The mapper receives entities
    ///     that have already been auto-mapped with matching properties, so you only need
    ///     to handle custom mapping logic.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         // Mapper with DI
    ///         public class UserMapper : IMappingConfigurationAsyncInstance&lt;User, UserDto&gt;
    ///         {
    ///             private readonly ILocationService _locationService;
    ///
    ///             public UserMapper(ILocationService locationService)
    ///             {
    ///                 _locationService = locationService;
    ///             }
    ///
    ///             public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    ///             {
    ///                 target.Position = new Vector2(source.X, source.Y);
    ///                 target.Location = await _locationService.GetLocationAsync(source.LocationId);
    ///             }
    ///         }
    ///
    ///         // Usage
    ///         var users = await dbContext.Users
    ///         .Where(u => u.IsActive)
    ///         .ToTargetsAsync&lt;User, UserDto&gt;(userMapper);
    ///     </code>
    /// </example>
    public static async Task<List<TTarget>> ToTargetsAsync<TSource, TTarget>(this IQueryable<TSource> source,
                                                                             IMappingConfigurationAsyncInstance<TSource,
                                                                                 TTarget> mapper,
                                                                             CancellationToken cancellationToken =
                                                                                 default)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        var sourceList = await source.ToListAsync(cancellationToken);

        return await sourceList.ToTargetsAsync(mapper, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously projects an IQueryable&lt;TSource&gt; to a List&lt;TTarget&gt;
    ///     using a static async mapper configuration. This is useful for complex mappings that
    ///     cannot be expressed as SQL projections.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type
    /// </typeparam>
    /// <typeparam name="TAsyncMapper">
    ///     The async mapper configuration type implementing IMappingConfigurationAsync&lt;TSource, TTarget&gt;
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token
    /// </param>
    /// <returns>
    ///     A list of mapped target instances
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source is null
    /// </exception>
    /// <remarks>
    ///     This method first executes the query to materialize the source entities,
    ///     then applies the custom mapper to each result. The mapper receives entities
    ///     that have already been auto-mapped with matching properties, so you only need
    ///     to handle custom mapping logic.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         // Static mapper
    ///         public class UserMapper : IMappingConfigurationAsync&lt;User, UserDto&gt;
    ///         {
    ///             public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    ///             {
    ///                 target.Position = new Vector2(source.X, source.Y);
    ///             }
    ///         }
    ///
    ///         // Usage
    ///         var users = await dbContext.Users
    ///         .Where(u => u.IsActive)
    ///         .ToTargetsAsync&lt;User, UserDto, UserMapper&gt;();
    ///     </code>
    /// </example>
    public static async Task<List<TTarget>> ToTargetsAsync<TSource, TTarget, TAsyncMapper>(
        this IQueryable<TSource> source,
        CancellationToken        cancellationToken = default)
        where TTarget : class
        where TAsyncMapper : IMappingConfigurationAsync<TSource, TTarget>
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var sourceList = await source.ToListAsync(cancellationToken);

        return await sourceList.ToTargetsAsync<TSource, TTarget, TAsyncMapper>(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously projects the first element of an IQueryable&lt;TSource&gt;
    ///     to a target using a custom async mapper instance, or returns null if none found.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source
    /// </param>
    /// <param name="mapper">
    ///     The async mapper instance (supports dependency injection)
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token
    /// </param>
    /// <returns>
    ///     The mapped target instance, or null if no source element found
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source or mapper is null
    /// </exception>
    /// <remarks>
    ///     This method first executes the query to get the first source entity,
    ///     then applies the custom mapper. The mapper receives an entity that has
    ///     already been auto-mapped with matching properties.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         var user = await dbContext.Users
    ///         .Where(u => u.Id == userId)
    ///         .FirstTargetAsync&lt;User, UserDto&gt;(userMapper);
    ///     </code>
    /// </example>
    public static async Task<TTarget?> FirstAsync<TSource, TTarget>(this IQueryable<TSource> source,
                                                                    IMappingConfigurationAsyncInstance<TSource, TTarget>
                                                                        mapper,
                                                                    CancellationToken cancellationToken = default)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        var sourceEntity = await source.FirstOrDefaultAsync(cancellationToken);

        if (sourceEntity == null) return null;

        return await sourceEntity.ToTargetAsync(mapper, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously projects the first element of an IQueryable&lt;TSource&gt;
    ///     to a target using a static async mapper configuration, or returns null if none found.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type
    /// </typeparam>
    /// <typeparam name="TAsyncMapper">
    ///     The async mapper configuration type implementing IMappingConfigurationAsync&lt;TSource, TTarget&gt;
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token
    /// </param>
    /// <returns>
    ///     The mapped target instance, or null if no source element found
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source is null
    /// </exception>
    /// <remarks>
    ///     This method first executes the query to get the first source entity,
    ///     then applies the custom mapper. The mapper receives an entity that has
    ///     already been auto-mapped with matching properties.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         var user = await dbContext.Users
    ///         .Where(u => u.Id == userId)
    ///         .FirstTargetAsync&lt;User, UserDto, UserMapper&gt;();
    ///     </code>
    /// </example>
    public static async Task<TTarget?> FirstAsync<TSource, TTarget, TAsyncMapper>(this IQueryable<TSource> source,
                                                                                  CancellationToken cancellationToken =
                                                                                      default)
        where TTarget : class
        where TAsyncMapper : IMappingConfigurationAsync<TSource, TTarget>
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var sourceEntity = await source.FirstOrDefaultAsync(cancellationToken);

        if (sourceEntity == null) return null;

        return await sourceEntity.ToTargetAsync<TSource, TTarget, TAsyncMapper>(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously projects a single element of an IQueryable&lt;TSource&gt;
    ///     to a target using a custom async mapper instance, throwing if not exactly one element exists.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source
    /// </param>
    /// <param name="mapper">
    ///     The async mapper instance (supports dependency injection)
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token
    /// </param>
    /// <returns>
    ///     The mapped target instance
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source or mapper is null
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the source sequence does not contain exactly one element
    /// </exception>
    /// <remarks>
    ///     This method first executes the query to get the single source entity,
    ///     then applies the custom mapper. The mapper receives an entity that has
    ///     already been auto-mapped with matching properties.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         var user = await dbContext.Users
    ///         .Where(u => u.Email == email)
    ///         .SingleAsync&lt;User, UserDto&gt;(userMapper);
    ///     </code>
    /// </example>
    public static async Task<TTarget> SingleAsync<TSource, TTarget>(this IQueryable<TSource> source,
                                                                    IMappingConfigurationAsyncInstance<TSource, TTarget>
                                                                        mapper,
                                                                    CancellationToken cancellationToken = default)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        var sourceEntity = await source.SingleAsync(cancellationToken);

        return await sourceEntity.ToTargetAsync(mapper, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously projects a single element of an IQueryable&lt;TSource&gt;
    ///     to a target using a static async mapper configuration, throwing if not exactly one element exists.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The source entity type
    /// </typeparam>
    /// <typeparam name="TTarget">
    ///     The target type
    /// </typeparam>
    /// <typeparam name="TAsyncMapper">
    ///     The async mapper configuration type implementing IMappingConfigurationAsync&lt;TSource, TTarget&gt;
    /// </typeparam>
    /// <param name="source">
    ///     The queryable source
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token
    /// </param>
    /// <returns>
    ///     The mapped target instance
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when source is null
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the source sequence does not contain exactly one element
    /// </exception>
    /// <remarks>
    ///     This method first executes the query to get the single source entity,
    ///     then applies the custom mapper. The mapper receives an entity that has
    ///     already been auto-mapped with matching properties.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         var user = await dbContext.Users
    ///         .Where(u => u.Email == email)
    ///         .SingleAsync&lt;User, UserDto, UserMapper&gt;();
    ///     </code>
    /// </example>
    public static async Task<TTarget> SingleAsync<TSource, TTarget, TAsyncMapper>(this IQueryable<TSource> source,
                                                                                  CancellationToken cancellationToken =
                                                                                      default)
        where TTarget : class
        where TAsyncMapper : IMappingConfigurationAsync<TSource, TTarget>
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        var sourceEntity = await source.SingleAsync(cancellationToken);

        return await sourceEntity.ToTargetAsync<TSource, TTarget, TAsyncMapper>(cancellationToken);
    }
}
