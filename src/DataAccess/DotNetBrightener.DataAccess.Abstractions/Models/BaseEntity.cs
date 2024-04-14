using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Models;

/// <summary>
///     Represents the base entity for all entities in the system, has the primary key as <typeparamref name="TKeyType"/>
/// </summary>
/// <typeparam name="TKeyType">
///     The type to use as the primary key of the entity
/// </typeparam>
public abstract class BaseEntity<TKeyType> : BaseEntity where TKeyType : struct
{
    /// <summary>
    ///     Identifier of the record, is also the primary key
    /// </summary>
    [Key]
    public new TKeyType Id { get; set; }

    protected BaseEntity()
    {
        if (typeof(TKeyType) == typeof(Guid))
        {
            Id = (TKeyType)(object)Guid.NewGuid();
        }
    }
}

public abstract class BaseEntity
{
    /// <summary>
    ///     Identifier of the record, is also the primary key
    /// </summary>
    [Key]
    public long Id { get; set; }
}