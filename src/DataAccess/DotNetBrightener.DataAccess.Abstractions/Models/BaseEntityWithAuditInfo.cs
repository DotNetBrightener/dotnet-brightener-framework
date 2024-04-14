using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Attributes;

namespace DotNetBrightener.DataAccess.Models;

public abstract class BaseEntityWithAuditInfo<TKeyType> : BaseEntity<TKeyType> where TKeyType : struct
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

public abstract class BaseEntityWithAuditInfo : BaseEntityWithAuditInfo<long>
{
}