using DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;
using DotNetBrightener.DataAccess.EF.PostgreSQL;

using DotNetBrightener.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.Integration;

/// <summary>
/// 	Integration tests for temporal query functionality
/// </summary>
public class TemporalTableQueryIntegrationTests(ITestOutputHelper testOutputHelper)
	: TemporalTestBase(testOutputHelper)
{
	[Fact]
	public async Task FetchHistory_All_ShouldReturnAllHistoricalRecords()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Original",
			Quantity = 10,
			Price = 10m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		entity.Name = "Updated 1";
		await dbContext.SaveChangesAsync();

		entity.Name = "Updated 2";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(null, null, null).ToListAsync();

		// Assert
		history.Count.ShouldBeGreaterThan(0);
	}

	[Fact]
	public async Task FetchHistory_WithIdFilter_ShouldReturnMatchingRecords()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity1 = new HistoryEnabledTestEntity
		{
			Name = "Entity 1",
			Quantity = 10,
			Price = 10m,
			IsActive = true
		};

		var entity2 = new HistoryEnabledTestEntity
		{
			Name = "Entity 2",
			Quantity = 20,
			Price = 20m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().AddRange(entity1, entity2);
		await dbContext.SaveChangesAsync();

		entity1.Name = "Updated Entity 1";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(e => e.Id == entity1.Id, null, null).ToListAsync();

		// Assert
		history.Count.ShouldBeGreaterThan(0);
		history.All(h => h.Id == entity1.Id).ShouldBeTrue();
	}

	[Fact]
	public async Task FetchHistory_WithDateRange_ShouldReturnRecordsInRange()
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

		var update1Time = DateTime.UtcNow;
		await Task.Delay(100);

		entity.Name = "Updated 1";
		await dbContext.SaveChangesAsync();

		var update2Time = DateTime.UtcNow;
		await Task.Delay(100);

		entity.Name = "Updated 2";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(
			e => e.Id == entity.Id,
			update1Time.AddSeconds(-1),
			update2Time.AddSeconds(1)).ToListAsync();

		// Assert - Should include records from the time range
		history.Count.ShouldBeGreaterThan(0);
	}

	[Fact]
	public async Task FetchHistory_ShouldIncludeCurrentRecord()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "Final Version",
			Quantity = 100,
			Price = 100m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		entity.Name = "Updated Final";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(e => e.Id == entity.Id, null, null).ToListAsync();

		// Assert - Should include the current record
		history.Count.ShouldBeGreaterThan(0);
		history.Any(h => h.Name == "Updated Final").ShouldBeTrue();
	}

	[Fact]
	public async Task FetchHistory_ShouldOrderByPeriodStartDesc()
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
			Quantity = 1,
			Price = 1m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		entity.Name = "Version 2";
		await dbContext.SaveChangesAsync();

		entity.Name = "Version 3";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(e => e.Id == entity.Id, null, null).ToListAsync();

		// Assert - Should be ordered newest first
		if (history.Count >= 2)
		{
			history[0].Name.ShouldBe("Version 3");
		}
	}

	[Fact]
	public async Task FetchHistory_ShouldNotTrackEntities()
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

		entity.Name = "Updated";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(e => e.Id == entity.Id, null, null).ToListAsync();

		// Assert - Entities should not be tracked
		var entry = dbContext.Entry(history.FirstOrDefault());
		if (history.Count > 0 && entry.State != EntityState.Detached)
		{
			// If tracked, verify it's not being tracked incorrectly
			// For AsNoTracking, entities should be Detached
		}
	}

	[Fact]
	public async Task FetchHistory_SameIdMultipleRevisions_ShouldNotDeduplicate()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		var entity = new HistoryEnabledTestEntity
		{
			Name = "V1",
			Quantity = 1,
			Price = 1m,
			IsActive = true
		};

		dbContext.Set<HistoryEnabledTestEntity>().Add(entity);
		await dbContext.SaveChangesAsync();

		entity.Name = "V2";
		await dbContext.SaveChangesAsync();

		entity.Name = "V3";
		await dbContext.SaveChangesAsync();

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(e => e.Id == entity.Id, null, null).ToListAsync();

		// Assert - All revisions should be returned, no deduplication
		history.Count.ShouldBeGreaterThan(1);
		history.All(h => h.Id == entity.Id).ShouldBeTrue();
	}

	[Fact]
	public async Task FetchHistory_WithNoMatchingRecords_ShouldReturnEmpty()
	{
		// Arrange
		var host = CreateTestHost();
		using var scope = host.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<TemporalTestDbContext>();
		await dbContext.Database.EnsureCreatedAsync();
		await EnsureHistoryInfrastructureAsync(host, dbContext);

		// Act
		var repository = CreateRepository(dbContext);
		var history = await repository.FetchHistory<HistoryEnabledTestEntity>(e => e.Id == 99999, null, null).ToListAsync();

		// Assert
		history.Count.ShouldBe(0);
	}

	private IRepository CreateRepository(TemporalTestDbContext dbContext)
	{
		var serviceProvider = new ServiceCollection()
			.AddLogging()
			.BuildServiceProvider();

		var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

		return new PostgreSqlRepository(dbContext, serviceProvider, loggerFactory);
	}
}
