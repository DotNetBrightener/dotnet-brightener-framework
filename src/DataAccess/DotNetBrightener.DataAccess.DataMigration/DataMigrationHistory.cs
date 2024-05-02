using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.DataAccess.DataMigration;

internal class DataMigrationHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(150)]
    public string MigrationId { get; set; }

    public DateTime? AppliedDateUtc { get; set; } = null!;
}