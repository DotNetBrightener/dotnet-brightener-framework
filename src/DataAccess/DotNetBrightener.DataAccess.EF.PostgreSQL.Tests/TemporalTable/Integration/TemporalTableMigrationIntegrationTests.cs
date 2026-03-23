using DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.Integration;

/// <summary>
/// 	Integration tests for temporal table schema creation
/// </summary>
public class TemporalTableMigrationIntegrationTests(ITestOutputHelper testOutputHelper)
	: TemporalTestBase(testOutputHelper)
{
	[Fact]
	public async Task EnsureCreated_ShouldCreateHistoryTable()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT EXISTS (
				SELECT FROM information_schema.tables
				WHERE table_schema = 'public'
				AND table_name = 'HistoryEnabledTestEntities_History'
			);";

		var result = await command.ExecuteScalarAsync();
		result.ShouldBe(true);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task EnsureCreated_ShouldCreateTriggerFunction()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT EXISTS (
				SELECT FROM pg_proc
				WHERE proname = 'HistoryEnabledTestEntities_history_trigger_func'
			);";

		var result = await command.ExecuteScalarAsync();
		result.ShouldBe(true);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task EnsureCreated_ShouldCreateTriggerOnMainTable()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT EXISTS (
				SELECT FROM pg_trigger
				WHERE tgname = 'HistoryEnabledTestEntities_history_trigger'
			);";

		var result = await command.ExecuteScalarAsync();
		result.ShouldBe(true);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryTable_ShouldHaveAllColumnsFromMainTable()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT column_name
			FROM information_schema.columns
			WHERE table_schema = 'public'
			AND table_name = 'HistoryEnabledTestEntities_History'
			ORDER BY column_name;";

		using var reader = await command.ExecuteReaderAsync();
		var columns = new List<string>();
		while (await reader.ReadAsync())
		{
			columns.Add(reader.GetString(0));
		}

		// Should have all main table columns plus period columns
		columns.ShouldContain("Id");
		columns.ShouldContain("Name");
		columns.ShouldContain("Quantity");
		columns.ShouldContain("Price");
		columns.ShouldContain("IsActive");
		columns.ShouldContain("ExpiryDate");
		columns.ShouldContain("CreatedDate");
		columns.ShouldContain("CreatedBy");
		columns.ShouldContain("ModifiedDate");
		columns.ShouldContain("ModifiedBy");
		columns.ShouldContain("IsDeleted");
		columns.ShouldContain("DeletedDate");
		columns.ShouldContain("DeletedBy");
		columns.ShouldContain("DeletionReason");
		columns.ShouldContain("PeriodStart");
		columns.ShouldContain("PeriodEnd");

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryTable_ShouldHavePeriodStartColumn()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT data_type
			FROM information_schema.columns
			WHERE table_schema = 'public'
			AND table_name = 'HistoryEnabledTestEntities_History'
			AND column_name = 'PeriodStart';";

		var result = await command.ExecuteScalarAsync();
		result.ShouldBe("timestamp with time zone");

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryTable_ShouldHavePeriodEndColumn()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT data_type
			FROM information_schema.columns
			WHERE table_schema = 'public'
			AND table_name = 'HistoryEnabledTestEntities_History'
			AND column_name = 'PeriodEnd';";

		var result = await command.ExecuteScalarAsync();
		result.ShouldBe("timestamp with time zone");

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryTable_ShouldHaveCompositePrimaryKey()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT a.attname
			FROM pg_index i
			JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
			WHERE i.indrelid = 'HistoryEnabledTestEntities_History'::regclass
			AND i.indisprimary;";

		using var reader = await command.ExecuteReaderAsync();
		var pkColumns = new List<string>();
		while (await reader.ReadAsync())
		{
			pkColumns.Add(reader.GetString(0));
		}

		// Should have composite PK on (Id, PeriodStart)
		pkColumns.Count.ShouldBe(2);
		pkColumns.ShouldContain("Id");
		pkColumns.ShouldContain("PeriodStart");

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryTable_ShouldHaveIndexesOnPeriodColumns()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		// Check for PeriodStart index
		using var command1 = connection.CreateCommand();
		command1.CommandText = @"
			SELECT EXISTS (
				SELECT FROM pg_indexes
				WHERE tablename = 'HistoryEnabledTestEntities_History'
				AND indexdef LIKE '%PeriodStart%'
			);";

		var hasPeriodStartIndex = await command1.ExecuteScalarAsync();
		((bool)hasPeriodStartIndex).ShouldBeTrue();

		// Check for PeriodEnd index
		using var command2 = connection.CreateCommand();
		command2.CommandText = @"
			SELECT EXISTS (
				SELECT FROM pg_indexes
				WHERE tablename = 'HistoryEnabledTestEntities_History'
				AND indexdef LIKE '%PeriodEnd%'
			);";

		var hasPeriodEndIndex = await command2.ExecuteScalarAsync();
		((bool)hasPeriodEndIndex).ShouldBeTrue();

		await connection.CloseAsync();
	}

	[Fact]
	public async Task NonHistoryEnabledEntity_ShouldNotHaveHistoryTable()
	{
		// Arrange
		var host = CreateTestHost();

		// Act
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT EXISTS (
				SELECT FROM information_schema.tables
				WHERE table_schema = 'public'
				AND table_name = 'NonHistoryEnabledTestEntities_History'
			);";

		var result = await command.ExecuteScalarAsync();
		result.ShouldBe(false);

		await connection.CloseAsync();
	}
}
