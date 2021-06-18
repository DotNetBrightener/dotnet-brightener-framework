using System;
using FluentMigrator.Runner.VersionTableInfo;

namespace DotNetBrightener.Core.DataAccess.Migration.Extensions
{
    public class SchemaMigrationHistory : IVersionTableMetaData
    {
        /// <summary>
        ///     Version
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        ///     Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Applied on date time
        /// </summary>
        public DateTime AppliedOn { get; set; }


        public object ApplicationContext { get; set; }

        public bool OwnsSchema => true;

        public string SchemaName => string.Empty;

        public string TableName => "___" + nameof(SchemaMigrationHistory);

        public string ColumnName => nameof(Version);

        public string DescriptionColumnName => nameof(Description);

        public string UniqueIndexName => "UC_Version";

        public string AppliedOnColumnName => nameof(AppliedOn);
    }
}
