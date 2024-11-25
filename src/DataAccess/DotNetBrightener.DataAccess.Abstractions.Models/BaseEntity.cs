using DotNetBrightener.DataAccess.Models.Utils.Internal;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Models;

public interface IBaseEntity
{

}
public interface IBaseEntity<TIdentifier> : IBaseEntity
{
    TIdentifier Id { get; set; }
}

/// <summary>
///     Represents the base entity for all entities in the system
/// </summary>
public abstract class BaseEntity<TIdentifier> : IBaseEntity<TIdentifier>
{
    /// <summary>
    ///     Identifier of the record, is also the primary key
    /// </summary>
    [Key]
    public TIdentifier Id { get; set; }
}

/// <summary>
///     Represents the base entity for all entities in the system
/// </summary>
public abstract class BaseEntity: BaseEntity<long>
{
}

/// <summary>
///     Represents the base entity for all entities in the system
/// </summary>
public abstract class GuidBaseEntity: BaseEntity<Guid>
{
    protected GuidBaseEntity()
    {
        Id = Uuid7.Guid();
    }
}