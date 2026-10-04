using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.Dapper.PostgreSQL;
using DotNetBrightener.TestHelpers.PostgreSql;
using Shouldly;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.Dapper.Tests;

public class NpgsqlDapperRepositoryTests(ITestOutputHelper testOutputHelper)
    : PostgreSqlServerBaseXUnitTest(testOutputHelper)
{
    private NpgsqlDapperRepository CreateRepository() =>
        new(new DatabaseConfiguration { ConnectionString = ConnectionString });

    private static Task EnsureTestTableAsync(NpgsqlDapperRepository repository) =>
        repository.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS widgets (
                id   SERIAL PRIMARY KEY,
                name TEXT NOT NULL
            );
            """);

    [Fact]
    public async Task ExecuteAsync_InsertsRow_VisibleToSubsequentRead()
    {
        var repository = CreateRepository();
        await EnsureTestTableAsync(repository);

        var affected = await repository.ExecuteAsync(
            "INSERT INTO widgets (name) VALUES (@Name)", new { Name = "gadget" });

        affected.ShouldBe(1);

        var name = await repository.ExecuteScalar<string>(
            "SELECT name FROM widgets WHERE name = @Name", new { Name = "gadget" });

        name.ShouldBe("gadget");
    }

    [Fact]
    public async Task UnitOfWork_CommitAsync_PersistsAllStatements()
    {
        var repository = CreateRepository();
        await EnsureTestTableAsync(repository);

        await using (var uow = await repository.BeginUnitOfWorkAsync())
        {
            await uow.ExecuteAsync("INSERT INTO widgets (name) VALUES (@Name)", new { Name = "a" });
            await uow.ExecuteAsync("INSERT INTO widgets (name) VALUES (@Name)", new { Name = "b" });

            await uow.CommitAsync();
        }

        var count = await repository.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM widgets WHERE name IN ('a', 'b')");

        count.ShouldBe(2);
    }

    [Fact]
    public async Task UnitOfWork_DisposeWithoutCommit_RollsBack()
    {
        var repository = CreateRepository();
        await EnsureTestTableAsync(repository);

        await using (var uow = await repository.BeginUnitOfWorkAsync())
        {
            await uow.ExecuteAsync("INSERT INTO widgets (name) VALUES (@Name)", new { Name = "uncommitted" });
            // No CommitAsync - dispose must roll back, not silently persist.
        }

        var count = await repository.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM widgets WHERE name = @Name", new { Name = "uncommitted" });

        count.ShouldBe(0);
    }

    [Fact]
    public async Task UnitOfWork_ExceptionMidway_RollsBackOnDispose()
    {
        var repository = CreateRepository();
        await EnsureTestTableAsync(repository);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await using var uow = await repository.BeginUnitOfWorkAsync();

            await uow.ExecuteAsync("INSERT INTO widgets (name) VALUES (@Name)", new { Name = "partial" });

            throw new InvalidOperationException("simulated failure mid-transaction");
        });

        var count = await repository.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM widgets WHERE name = @Name", new { Name = "partial" });

        count.ShouldBe(0);
    }
}
