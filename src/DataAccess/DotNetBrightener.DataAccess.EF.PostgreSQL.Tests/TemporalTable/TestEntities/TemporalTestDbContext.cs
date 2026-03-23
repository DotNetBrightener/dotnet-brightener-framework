using DotNetBrightener.DataAccess.EF.PostgreSQL;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;

/// <summary>
/// 	Test DbContext for temporal table testing
/// </summary>
public class TemporalTestDbContext : PostgreSqlVersioningMigrationEnabledDbContext
{
	/// <summary>
	/// 	Initializes a new instance of the <see cref="TemporalTestDbContext"/> class
	/// </summary>
	public TemporalTestDbContext(DbContextOptions<TemporalTestDbContext> options)
		: base(options)
	{
	}

	/// <summary>
	/// 	Configure the model builder
	/// </summary>
	protected override void ConfigureModelBuilder(ModelBuilder modelBuilder)
	{
		// Configure HistoryEnabledTestEntity
		modelBuilder.Entity<HistoryEnabledTestEntity>(entity =>
		{
			entity.ToTable("HistoryEnabledTestEntities");
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
			entity.Property(x => x.Price).HasPrecision(18, 2);
		});

		// Configure NonHistoryEnabledTestEntity
		modelBuilder.Entity<NonHistoryEnabledTestEntity>(entity =>
		{
			entity.ToTable("NonHistoryEnabledTestEntities");
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Value).IsRequired();
		});
	}
}
