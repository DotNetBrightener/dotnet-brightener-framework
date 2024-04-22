using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Models;

/// <summary>
///     Represents the base entity for all entities in the system
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    ///     Identifier of the record, is also the primary key
    /// </summary>
    [Key]
    public long Id { get; set; }
}