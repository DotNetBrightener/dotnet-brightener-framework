using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.Core.DataAccess.Abstractions
{
    /// <summary>
    ///     Represents the base properties of an entity
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        ///     The Identity of the record
        /// </summary>
        [Key]
        public long Id { get; set; }
    }
}