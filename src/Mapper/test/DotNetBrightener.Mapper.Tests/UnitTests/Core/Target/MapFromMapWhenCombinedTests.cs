namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entity - moved to separate namespace to avoid nesting issues
public class CombinedTestEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public MapCombinedOrderStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum MapCombinedOrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

// Test: MapFrom + MapWhen on same property
[MappingTarget<CombinedTestEntity>()]
public partial class CombinedMapFromMapWhenTarget
{
    // Rename property AND apply conditional mapping
    [MapFrom(nameof(CombinedTestEntity.FirstName))]
    [MapWhen("IsActive")]
    public string? DisplayName { get; set; }
}

// Test: MapFrom rename with multiple MapWhen conditions
[MappingTarget<CombinedTestEntity>()]
public partial class CombinedMultipleConditionsTarget
{
    [MapFrom(nameof(CombinedTestEntity.Email))]
    [MapWhen("IsActive")]
    [MapWhen("IsEmailVerified")]
    public string? VerifiedEmail { get; set; }
}

// Test: MapFrom + MapWhen with status check
[MappingTarget<CombinedTestEntity>()]
public partial class CombinedStatusCheckTarget
{
    [MapFrom(nameof(CombinedTestEntity.CompletedAt))]
    [MapWhen("Status == MapCombinedOrderStatus.Completed")]
    public DateTime? FinishedAt { get; set; }
}

/// <summary>
///     Tests for combining [MapFrom] and [MapWhen] attributes on the same property.
/// </summary>
public class MapFromMapWhenCombinedTests
{
    [Fact]
    public void Constructor_ShouldApplyBothMapFromAndMapWhen_WhenConditionIsTrue()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        // Act
        var target = new CombinedMapFromMapWhenTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.DisplayName.ShouldBe("John"); // Mapped from FirstName, condition IsActive = true
    }

    [Fact]
    public void Constructor_ShouldNotMap_WhenMapWhenConditionIsFalse()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            IsActive = false // Condition is false
        };

        // Act
        var target = new CombinedMapFromMapWhenTarget(entity);

        // Assert
        target.Id.ShouldBe(1);
        target.DisplayName.ShouldBeNull(); // Not mapped because IsActive = false
    }

    [Fact]
    public void Constructor_ShouldApplyMultipleConditions_AllTrue()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            Email = "john@example.com",
            IsActive = true,
            IsEmailVerified = true
        };

        // Act
        var target = new CombinedMultipleConditionsTarget(entity);

        // Assert
        target.VerifiedEmail.ShouldBe("john@example.com");
    }

    [Fact]
    public void Constructor_ShouldNotMap_WhenAnyConditionIsFalse()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            Email = "john@example.com",
            IsActive = true,
            IsEmailVerified = false // One condition is false
        };

        // Act
        var target = new CombinedMultipleConditionsTarget(entity);

        // Assert
        target.VerifiedEmail.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldApplyMapFromRenameWithStatusCondition()
    {
        // Arrange
        var completedTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var entity = new CombinedTestEntity
        {
            Id = 1,
            Status = MapCombinedOrderStatus.Completed,
            CompletedAt = completedTime
        };

        // Act
        var target = new CombinedStatusCheckTarget(entity);

        // Assert
        target.FinishedAt.ShouldBe(completedTime); // Renamed from CompletedAt
    }

    [Fact]
    public void Constructor_ShouldNotMapRenamedProperty_WhenStatusConditionFails()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            Status = MapCombinedOrderStatus.Pending,
            CompletedAt = DateTime.Now
        };

        // Act
        var target = new CombinedStatusCheckTarget(entity);

        // Assert
        target.FinishedAt.ShouldBeNull(); // Not mapped because Status != Completed
    }

    [Fact]
    public void Projection_ShouldApplyBothMapFromAndMapWhen()
    {
        // Arrange
        var entities = new[]
        {
            new CombinedTestEntity { Id = 1, FirstName = "Active", IsActive = true },
            new CombinedTestEntity { Id = 2, FirstName = "Inactive", IsActive = false }
        }.AsQueryable();

        // Act
        var targets = entities.Select(CombinedMapFromMapWhenTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].DisplayName.ShouldBe("Active"); // Condition true
        targets[1].DisplayName.ShouldBeNull(); // Condition false
    }

    [Fact]
    public void Projection_ShouldApplyMultipleConditionsWithMapFrom()
    {
        // Arrange
        var entities = new[]
        {
            new CombinedTestEntity { Id = 1, Email = "verified@example.com", IsActive = true, IsEmailVerified = true },
            new CombinedTestEntity { Id = 2, Email = "unverified@example.com", IsActive = true, IsEmailVerified = false },
            new CombinedTestEntity { Id = 3, Email = "inactive@example.com", IsActive = false, IsEmailVerified = true }
        }.AsQueryable();

        // Act
        var targets = entities.Select(CombinedMultipleConditionsTarget.Projection).ToList();

        // Assert
        targets.Count().ShouldBe(3);
        targets[0].VerifiedEmail.ShouldBe("verified@example.com"); // Both conditions true
        targets[1].VerifiedEmail.ShouldBeNull(); // IsEmailVerified = false
        targets[2].VerifiedEmail.ShouldBeNull(); // IsActive = false
    }

    [Fact]
    public void TargetType_ShouldHaveCorrectPropertyNames()
    {
        // Arrange
        var targetType = typeof(CombinedMapFromMapWhenTarget);
        var propertyNames = targetType.GetProperties().Select(p => p.Name).ToList();

        // Assert
        propertyNames.ShouldContain("DisplayName"); // Renamed property
        propertyNames.ShouldNotContain("FirstName"); // Original name should not exist
    }

    #region Edge Cases

    [Fact]
    public void Constructor_ShouldHandleEmptyStrings_WithMapFromAndMapWhen()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            FirstName = "",
            IsActive = true
        };

        // Act
        var target = new CombinedMapFromMapWhenTarget(entity);

        // Assert
        target.DisplayName.ShouldBe("");
    }

    [Fact]
    public void Constructor_ShouldHandleNullEmail_WithMultipleConditions()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            Email = null,
            IsActive = true,
            IsEmailVerified = true
        };

        // Act
        var target = new CombinedMultipleConditionsTarget(entity);

        // Assert
        target.VerifiedEmail.ShouldBeNull();
    }

    [Fact]
    public void Projection_ShouldHandleAllStatusValues()
    {
        // Arrange
        var entities = new[]
        {
            new CombinedTestEntity { Id = 1, Status = MapCombinedOrderStatus.Pending, CompletedAt = DateTime.Now },
            new CombinedTestEntity { Id = 2, Status = MapCombinedOrderStatus.Processing, CompletedAt = DateTime.Now },
            new CombinedTestEntity { Id = 3, Status = MapCombinedOrderStatus.Completed, CompletedAt = new DateTime(2024, 6, 15) },
            new CombinedTestEntity { Id = 4, Status = MapCombinedOrderStatus.Cancelled, CompletedAt = DateTime.Now }
        }.AsQueryable();

        // Act
        var targets = entities.Select(CombinedStatusCheckTarget.Projection).ToList();

        // Assert
        targets[0].FinishedAt.ShouldBeNull();
        targets[1].FinishedAt.ShouldBeNull();
        targets[2].FinishedAt.ShouldBe(new DateTime(2024, 6, 15));
        targets[3].FinishedAt.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldNotMapWhenBothConditionsFalse()
    {
        // Arrange
        var entity = new CombinedTestEntity
        {
            Id = 1,
            Email = "test@example.com",
            IsActive = false,
            IsEmailVerified = false
        };

        // Act
        var target = new CombinedMultipleConditionsTarget(entity);

        // Assert
        target.VerifiedEmail.ShouldBeNull();
    }

    #endregion
}
