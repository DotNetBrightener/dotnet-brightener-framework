using System.Reflection;

namespace DotNetBrightener.Core.DataAccess.Migration.Abstractions.Services
{
    public interface ISchemaMigrationManager
    {
        void ApplyUpMigrations(Assembly assembly);

        void ApplyDownMigrations(Assembly assembly);
    }
}
