using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Attributes;

namespace DotNetBrightener.DataAccess.Models;

public interface IAuditableEntity: IBaseEntity
{
    /// <summary>
    ///     The date and time of record creation
    /// </summary>
    DateTimeOffset? CreatedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who created the record
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    ///     The date and time of last modification of the record
    /// </summary>
    DateTimeOffset? ModifiedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who modified the record
    /// </summary>
    string ModifiedBy { get; set; }

    /// <summary>
    ///     Indicates the entity record is marked as deleted
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    ///     The date and time of record deletion
    /// </summary>
    DateTimeOffset? DeletedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who deleted the record
    /// </summary>
    string DeletedBy { get; set; }

    /// <summary>
    ///     The reason of why the record is deleted
    /// </summary>
    string DeletionReason { get; set; }
}

/// <summary>
///     Represents the entities with auditable information fields, and has the primary key type of <see cref="long"/>
/// </summary>
public abstract class BaseEntityWithAuditInfo : BaseEntityWithAuditInfo<long>
{
}


/// <summary>
///     Represents the entities with auditable information fields, and has the primary key type of <see cref="TIdentifier"/>
/// </summary>
/// <typeparam name="TIdentifier">
///     The type of the identifier property, or the primary key of the table
/// </typeparam>
public abstract class BaseEntityWithAuditInfo<TIdentifier> : BaseEntity<TIdentifier>, IAuditableEntity
{
    /// <summary>
    ///     The date and time of record creation
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? CreatedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who created the record
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string CreatedBy { get; set; }

    /// <summary>
    ///     The date and time of last modification of the record
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? ModifiedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who modified the record
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string ModifiedBy { get; set; }

    /// <summary>
    ///     Indicates the entity record is marked as deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     The date and time of record deletion
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? DeletedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who deleted the record
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string DeletedBy { get; set; }

    /// <summary>
    ///     The reason of why the record is deleted
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(512)]
    public string DeletionReason { get; set; }
}

/// <summary>
///     Represents the entities with auditable information fields, and has the primary key type of <see cref="Guid"/>
/// </summary>
public abstract class GuidBaseEntityWithAuditInfo : GuidBaseEntity, IAuditableEntity
{
    /// <summary>
    ///     The date and time of record creation
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? CreatedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who created the record
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string CreatedBy { get; set; }

    /// <summary>
    ///     The date and time of last modification of the record
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? ModifiedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who modified the record
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string ModifiedBy { get; set; }

    /// <summary>
    ///     Indicates the entity record is marked as deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     The date and time of record deletion
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? DeletedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who deleted the record
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string DeletedBy { get; set; }

    /// <summary>
    ///     The reason of why the record is deleted
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(512)]
    public string DeletionReason { get; set; }
}