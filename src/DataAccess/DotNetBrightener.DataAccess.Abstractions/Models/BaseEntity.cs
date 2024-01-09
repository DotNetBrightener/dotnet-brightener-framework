﻿using DotNetBrightener.DataAccess.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.Models;

public abstract class BaseEntity
{
    /// <summary>
    ///     Identifier of the record, is also the primary key
    /// </summary>
    [Key]
    public long Id { get; set; }
}

public abstract class BaseEntityWithAuditInfo : BaseEntity
{
    /// <summary>
    ///     Indicates the entity record is marked as deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    ///     The date and time of record creation
    /// </summary>
    [NoClientSideUpdate]
    public DateTimeOffset? CreatedDate { get; set; }

    /// <summary>
    ///     The name or identifier of the user who created the entity
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
    ///     The name or identifier of the user who modified the entity
    /// </summary>
    [NoClientSideUpdate]
    [MaxLength(255)]
    public string ModifiedBy { get; set; }
}