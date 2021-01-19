using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.Core.DataAccess.Abstractions
{
    public abstract class BaseEntityWithAuditInfo: BaseEntity
    {
        /// <summary>
        ///     Indicates when the record was created
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        ///     Indicates by whom the record was created
        /// </summary>
        [MaxLength(512)]
        public string CreatedBy { get; set; }

        /// <summary>
        ///     Indicates when the record was last updated
        /// </summary>
        public DateTimeOffset? LastUpdated { get; set; }

        /// <summary>
        ///     Indicates by whom the record was last updated
        /// </summary>
        [MaxLength(512)]
        public string LastUpdatedBy { get; set; }

        /// <summary>
        ///     Indicates if the record is marked as deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        ///     Indicates when the record was marked as deleted
        /// </summary>
        public DateTimeOffset? Deleted { get; set; }
    }
}