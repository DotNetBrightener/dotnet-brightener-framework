using System;

namespace DotNetBrightener.Integration.DataMigration
{
    public class DataMigrationAttribute : Attribute
    {
        public string MigrationId { get; set; }
    }
}