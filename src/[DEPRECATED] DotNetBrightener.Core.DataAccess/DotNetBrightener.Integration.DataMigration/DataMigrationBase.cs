using System;
using System.Threading.Tasks;
using DotNetBrightener.Core.Modular;

namespace DotNetBrightener.Integration.DataMigration
{
    /// <summary>
    ///     Represents the instruction for data migration
    /// </summary>
    public abstract class DataMigrationBase
    {
        /// <summary>
        ///     Specifies the version for the migration
        /// </summary>
        internal abstract string MigrationId { get; }

        internal string ModuleId { get; private set; }

        internal ModuleEntry ModuleEntry { get; set; }

        /// <summary>
        ///     Prepares the migration. Needs to be called before <see cref="PerformUpgrade"/> method is called
        /// </summary>
        /// <param name="associatedModule">
        ///     The module in which the migration belongs to.
        /// </param>
        internal void Prepare(ModuleEntry associatedModule)
        {
            ModuleId = associatedModule.ModuleId;
            ModuleEntry = associatedModule;
        }

        internal Task InternalUpgrade()
        {
            if (string.IsNullOrEmpty(ModuleId))
            {
                throw new InvalidOperationException($"The migration is not ready, make sure you call {nameof(Prepare)}() method before performing this operation.");
            }

            return PerformUpgrade();
        }

        protected abstract Task PerformUpgrade();
    }

    public abstract class DataMigration : DataMigrationBase
    {
        /// <summary>
        ///     Defines the version for the migration instance by specifying the timestamp of creating the migration
        /// </summary>
        /// <remarks>
        ///     The timestamp should be in format of yyyyMMddHHmmss
        /// </remarks>
        protected abstract string VersionTimestamp { get; }

        internal sealed override string MigrationId => $"{VersionTimestamp}_{GetType().Name}";
    }
}