namespace DotNetBrightener.DataAccess.DataMigration.Tests;

internal class ShouldNotBeRegisteredMigration : IDataMigration
{
    public Task MigrateData()
    {
        return Task.CompletedTask;
    }
}

[DataMigration("20240502_160412_InitializeMigration")]
internal class GoodMigration : IDataMigration
{
    public Task MigrateData()
    {
        return Task.CompletedTask;
    }
}

[DataMigration("20240502_160413_InitializeMigration2")]
internal class MigrationWithThrowingException : IDataMigration
{
    public Task MigrateData()
    {
        throw new InvalidOperationException("Just to break the test");
    }
}

[DataMigration("20240502_160413_InitializeMigration3")]
internal class GoodMigration2 : IDataMigration
{
    public Task MigrateData()
    {
        return Task.CompletedTask;
    }
}