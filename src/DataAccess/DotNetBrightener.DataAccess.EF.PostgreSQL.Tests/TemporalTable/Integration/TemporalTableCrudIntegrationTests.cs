using DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.Integration;

/// <summary>
/// 	Integration tests for CRUD operations with history tracking
/// </summary>
public class TemporalTableCrudIntegrationTests(ITestOutputHelper testOutputHelper)
	: TemporalTestBase(testOutputHelper)
{
	[Fact]
	public async Task Insert_ShouldNotCreateHistoryEntry()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Test Product",
			Quantity = 100,
			Price = 29.99m,
			IsActive = true
		};

		// Act
		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT COUNT(*)
			FROM ""HistoryEnabledTestEntities_History""
			WHERE ""Id"" = @Id;";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "@Id";
		parameter.Value = entity.Id;
		command.Parameters.Add(parameter);

		var historyCount = await command.ExecuteScalarAsync();
		historyCount.ShouldBe(0);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task Update_ShouldCreateHistoryEntryWithOldValues()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Original Name",
			Quantity = 50,
			Price = 19.99m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		var originalId = entity.Id;

		// Act
		entity.Name = "Updated Name";
		entity.Price = 29.99m;
		await dbContext.SaveChangesAsync();

		// Assert
		await using var dbContext2 = CreateTestHost().Services.CreateScope().ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		var connection = dbContext2.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT ""Name"", ""Price"", ""PeriodStart"", ""PeriodEnd""
			FROM ""HistoryEnabledTestEntities_History""
			WHERE ""Id"" = @Id
			ORDER BY ""PeriodStart"" DESC;";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "@Id";
		parameter.Value = originalId;
		command.Parameters.Add(parameter);

		using var reader = await command.ExecuteReaderAsync();
		reader.Read().ShouldBeTrue();

		var historyName = reader.GetString(0);
		var historyPrice = reader.GetDecimal(1);

		historyName.ShouldBe("Original Name");
		historyPrice.ShouldBe(19.99m);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task UpdateMultipleTimes_ShouldCreateMultipleHistoryEntries()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Version 1",
			Quantity = 10,
			Price = 10m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		// Act
		entity.Name = "Version 2";
		await dbContext.SaveChangesAsync();

		entity.Name = "Version 3";
		await dbContext.SaveChangesAsync();

		entity.Name = "Version 4";
		await dbContext.SaveChangesAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT COUNT(*)
			FROM ""HistoryEnabledTestEntities_History""
			WHERE ""Id"" = @Id;";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "@Id";
		parameter.Value = entity.Id;
		command.Parameters.Add(parameter);

		var historyCount = await command.ExecuteScalarAsync();
		historyCount.ShouldBe(3);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task Delete_ShouldCreateHistoryEntryBeforeDeletion()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "To Be Deleted",
			Quantity = 5,
			Price = 99.99m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		var entityId = entity.Id;

		// Act
		dbContext.Set<HistoryEnabledTestEntity>().Remove(entity);
		await dbContext.SaveChangesAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT ""Name"", ""IsActive""
			FROM ""HistoryEnabledTestEntities_History""
			WHERE ""Id"" = @Id;";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "@Id";
		parameter.Value = entityId;
		command.Parameters.Add(parameter);

		using var reader = await command.ExecuteReaderAsync();
		reader.Read().ShouldBeTrue();

		var historyName = reader.GetString(0);
		var historyIsActive = reader.GetBoolean(1);

		historyName.ShouldBe("To Be Deleted");
		historyIsActive.ShouldBe(true);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryEntry_ShouldHaveValidPeriodStart()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Test",
			Quantity = 1,
			Price = 1m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		var updateTime = DateTime.UtcNow;

		// Act
		entity.Name = "Updated";
		await dbContext.SaveChangesAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT ""PeriodStart""
			FROM ""HistoryEnabledTestEntities_History""
			WHERE ""Id"" = @Id;";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "@Id";
		parameter.Value = entity.Id;
		command.Parameters.Add(parameter);

		var periodStart = await command.ExecuteScalarAsync();
		periodStart.ShouldNotBeNull();

		var startTime = (DateTime)periodStart;
		(startTime - updateTime).TotalSeconds.ShouldBeLessThan(5);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task HistoryEntry_ShouldHaveMaxPeriodEnd()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Test",
			Quantity = 1,
			Price = 1m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		// Act
		entity.Name = "Updated";
		await dbContext.SaveChangesAsync();

		// Assert
		var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT ""PeriodEnd""
			FROM ""HistoryEnabledTestEntities_History""
			WHERE ""Id"" = @Id;";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "@Id";
		parameter.Value = entity.Id;
		command.Parameters.Add(parameter);

		var periodEnd = await command.ExecuteScalarAsync();
		periodEnd.ShouldNotBeNull();

		var endTime = (DateTime)periodEnd;
		endTime.Year.ShouldBeGreaterThan(2500);

		await connection.CloseAsync();
	}

	[Fact]
	public async Task NonHistoryEnabledEntity_ShouldNotCreateHistoryEntry()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new NonHistoryEnabledTestEntity
		{
			Value = "Test Value"
		};

		// Act
		dbContext.Set<NonHistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		entity.Value = "Updated Value";
		await dbContext.SaveChangesAsync();

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
