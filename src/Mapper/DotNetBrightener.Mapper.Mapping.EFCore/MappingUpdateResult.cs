using System.Collections.Generic;

namespace DotNetBrightener.Mapper.Mapping.EFCore;

/// <summary>
///     Represents the result of a target update operation, containing the updated entity and information about what changed.
/// </summary>
/// <typeparam name="TEntity">
///     The type of entity that was updated
/// </typeparam>
public readonly record struct MappingUpdateResult<TEntity>(
    TEntity               Entity,
    IReadOnlyList<string> ChangedProperties)
    where TEntity : class
{
    /// <summary>
    ///     Gets a value indicating whether any properties were changed during the update.
    /// </summary>
    public bool HasChanges => ChangedProperties.Count > 0;
}
