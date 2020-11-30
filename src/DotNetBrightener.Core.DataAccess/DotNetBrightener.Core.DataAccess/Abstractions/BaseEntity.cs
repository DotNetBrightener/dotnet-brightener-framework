using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.Core.DataAccess.Abstractions
{
    public abstract class BaseEntity
    {
        [Description("The Identity of the record")]
        [Key]
        public long Id { get; set; }
    }
}