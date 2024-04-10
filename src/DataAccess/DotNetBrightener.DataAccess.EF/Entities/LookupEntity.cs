using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.DataAccess.EF.Entities;

internal class EnumLookupEntity<TEnum> where TEnum : struct, Enum
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [MaxLength(1024)]
    public string Value { get; set; }
}