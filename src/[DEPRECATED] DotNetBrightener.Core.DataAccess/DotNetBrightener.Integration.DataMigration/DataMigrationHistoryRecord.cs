using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetBrightener.Integration.DataMigration
{
    [Table("__DataMigrationHistories")]
    public class DataMigrationHistoryRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MigrationId { get; set; }

        public string ModuleId { get; set; }

        public DateTimeOffset? MigrationRecorded { get; set; } = DateTimeOffset.Now;
    }
}