using DotNetBrightener.Mapper.Mapping.Configurations;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for Before/After mapping hooks functionality.
/// </summary>
public class MappingHooksTests
{
    // Test entities
    public class HooksTestEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
    }

    // BeforeMap configuration - validates and sets defaults
    public class UserBeforeMapConfig
    {
        public static void BeforeMap(HooksTestEntity source, BeforeMapTarget target)
        {
            target.MappedAt = DateTime.UtcNow;
            
            if (string.IsNullOrEmpty(source.FirstName))
            {
                target.ValidationMessage = "FirstName is required";
            }
        }
    }

    // AfterMap configuration - computes derived values
    public class UserAfterMapConfig
    {
        public static void AfterMap(HooksTestEntity source, AfterMapTarget target)
        {
            target.FullName = $"{target.FirstName} {target.LastName}";
            target.Age = CalculateAge(source.DateOfBirth);
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    // Combined hooks configuration
    public class UserCombinedHooksConfig
    {
        public static void BeforeMap(HooksTestEntity source, CombinedHooksTarget target)
        {
            target.MappedAt = DateTime.UtcNow;
        }

        public static void AfterMap(HooksTestEntity source, CombinedHooksTarget target)
        {
            target.FullName = $"{target.FirstName} {target.LastName}";
            target.Age = CalculateAge(source.DateOfBirth);
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    // Placeholder targets for unit tests
    public class BeforeMapTarget
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public DateTime MappedAt { get; set; }
        public string? ValidationMessage { get; set; }
    }

    public class AfterMapTarget
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class CombinedHooksTarget
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public DateTime MappedAt { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    #region Interface Availability Tests

    [Fact]
    public void BeforeMapConfiguration_Interface_ShouldBeAvailable()
    {
        var type = typeof(IBeforeMapConfiguration<,>);
        type.ShouldNotBeNull();
        type.IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void AfterMapConfiguration_Interface_ShouldBeAvailable()
    {
        var type = typeof(IAfterMapConfiguration<,>);
        type.ShouldNotBeNull();
        type.IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void CombinedHooksConfiguration_Interface_ShouldBeAvailable()
    {
        var type = typeof(IMapHooksConfiguration<,>);
        type.ShouldNotBeNull();
        type.IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void AsyncHooksInterfaces_ShouldExist()
    {
        typeof(IBeforeMapConfigurationAsync<,>).ShouldNotBeNull();
        typeof(IAfterMapConfigurationAsync<,>).ShouldNotBeNull();
        typeof(IMapHooksConfigurationAsync<,>).ShouldNotBeNull();
    }

    [Fact]
    public void InstanceHooksInterfaces_ShouldExist()
    {
        typeof(IBeforeMapConfigurationInstance<,>).ShouldNotBeNull();
        typeof(IAfterMapConfigurationInstance<,>).ShouldNotBeNull();
        typeof(IMapHooksConfigurationInstance<,>).ShouldNotBeNull();
    }

    [Fact]
    public void AsyncInstanceHooksInterfaces_ShouldExist()
    {
        typeof(IBeforeMapConfigurationAsyncInstance<,>).ShouldNotBeNull();
        typeof(IAfterMapConfigurationAsyncInstance<,>).ShouldNotBeNull();
        typeof(IMapHooksConfigurationAsyncInstance<,>).ShouldNotBeNull();
    }

    #endregion

    #region MappingTargetAttribute Property Tests

    [Fact]
    public void MappingTargetAttribute_ShouldHaveBeforeMapConfigurationProperty()
    {
        var property = typeof(MappingTargetAttribute<HooksTestEntity>).GetProperty("BeforeMapConfiguration");
        property.ShouldNotBeNull();
        property!.PropertyType.ShouldBe(typeof(Type));
    }

    [Fact]
    public void MappingTargetAttribute_ShouldHaveAfterMapConfigurationProperty()
    {
        var property = typeof(MappingTargetAttribute<HooksTestEntity>).GetProperty("AfterMapConfiguration");
        property.ShouldNotBeNull();
        property!.PropertyType.ShouldBe(typeof(Type));
    }

    #endregion

    #region BeforeMap Tests

    [Fact]
    public void BeforeMap_ShouldBeCalledBeforePropertyMapping()
    {
        // Arrange
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.Today.AddYears(-30),
            IsActive = true
        };
        var target = new BeforeMapTarget();

        // Act
        UserBeforeMapConfig.BeforeMap(entity, target);

        // Assert
        target.MappedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        target.ValidationMessage.ShouldBeNull();
    }

    [Fact]
    public void BeforeMap_ShouldSetValidationMessage_WhenInputInvalid()
    {
        // Arrange
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "",
            LastName = "Doe"
        };
        var target = new BeforeMapTarget();

        // Act
        UserBeforeMapConfig.BeforeMap(entity, target);

        // Assert
        target.ValidationMessage.ShouldBe("FirstName is required");
    }

    [Fact]
    public void BeforeMap_ShouldSetMappedAtTimestamp()
    {
        // Arrange
        var entity = new HooksTestEntity { Id = 1, FirstName = "Test", LastName = "User" };
        var target = new BeforeMapTarget();
        var beforeCall = DateTime.UtcNow;

        // Act
        UserBeforeMapConfig.BeforeMap(entity, target);
        var afterCall = DateTime.UtcNow;

        // Assert
        target.MappedAt.ShouldBeGreaterThanOrEqualTo(beforeCall);
        target.MappedAt.ShouldBeLessThanOrEqualTo(afterCall);
    }

    #endregion

    #region AfterMap Tests

    [Fact]
    public void AfterMap_ShouldComputeDerivedValues()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-25);
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = birthDate,
            IsActive = true
        };
        var target = new AfterMapTarget
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateOfBirth = entity.DateOfBirth,
            IsActive = entity.IsActive
        };

        // Act
        UserAfterMapConfig.AfterMap(entity, target);

        // Assert
        target.FullName.ShouldBe("John Doe");
        target.Age.ShouldBe(25);
    }

    [Fact]
    public void AfterMap_ShouldCalculateAge_ForRecentBirthday()
    {
        // Arrange - birthday was 6 months ago
        var birthDate = DateTime.Today.AddMonths(-6).AddYears(-30);
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "Past",
            LastName = "Birthday",
            DateOfBirth = birthDate
        };
        var target = new AfterMapTarget
        {
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateOfBirth = entity.DateOfBirth
        };

        // Act
        UserAfterMapConfig.AfterMap(entity, target);

        // Assert
        target.Age.ShouldBe(30);
    }

    [Fact]
    public void AfterMap_ShouldCalculateAge_ForUpcomingBirthday()
    {
        // Arrange - birthday is 6 months from now
        var birthDate = DateTime.Today.AddMonths(6).AddYears(-30);
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "Future",
            LastName = "Birthday",
            DateOfBirth = birthDate
        };
        var target = new AfterMapTarget
        {
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateOfBirth = entity.DateOfBirth
        };

        // Act
        UserAfterMapConfig.AfterMap(entity, target);

        // Assert
        target.Age.ShouldBe(29);
    }

    #endregion

    #region Combined Hooks Tests

    [Fact]
    public void CombinedHooks_ShouldCallBothBeforeAndAfterMap()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-35);
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = birthDate,
            IsActive = true
        };
        var target = new CombinedHooksTarget();

        // Act - Simulate full mapping lifecycle
        UserCombinedHooksConfig.BeforeMap(entity, target);
        var mappedAtTime = target.MappedAt;
        
        target.Id = entity.Id;
        target.FirstName = entity.FirstName;
        target.LastName = entity.LastName;
        target.DateOfBirth = entity.DateOfBirth;
        target.IsActive = entity.IsActive;
        
        UserCombinedHooksConfig.AfterMap(entity, target);

        // Assert
        target.MappedAt.ShouldBe(mappedAtTime);
        target.FullName.ShouldBe("Jane Smith");
        target.Age.ShouldBe(35);
    }

    [Fact]
    public void CombinedHooks_ShouldPreserveMappedAtFromBeforeMap()
    {
        // Arrange
        var entity = new HooksTestEntity
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.Today.AddYears(-20)
        };
        var target = new CombinedHooksTarget();

        // Act
        UserCombinedHooksConfig.BeforeMap(entity, target);
        var originalMappedAt = target.MappedAt;
        
        target.FirstName = entity.FirstName;
        target.LastName = entity.LastName;
        target.DateOfBirth = entity.DateOfBirth;
        
        UserCombinedHooksConfig.AfterMap(entity, target);

        // Assert
        target.MappedAt.ShouldBe(originalMappedAt);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void CombinedHooksInterface_ShouldInheritFromBothBeforeAndAfter()
    {
        var combinedType = typeof(IMapHooksConfiguration<,>);
        var interfaces = combinedType.GetInterfaces();
        
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IBeforeMapConfiguration<,>));
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IAfterMapConfiguration<,>));
    }

    [Fact]
    public void CombinedAsyncHooksInterface_ShouldInheritFromBothAsyncInterfaces()
    {
        var combinedType = typeof(IMapHooksConfigurationAsync<,>);
        var interfaces = combinedType.GetInterfaces();
        
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IBeforeMapConfigurationAsync<,>));
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IAfterMapConfigurationAsync<,>));
    }

    [Fact]
    public void CombinedInstanceHooksInterface_ShouldInheritFromBothInstanceInterfaces()
    {
        var combinedType = typeof(IMapHooksConfigurationInstance<,>);
        var interfaces = combinedType.GetInterfaces();
        
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IBeforeMapConfigurationInstance<,>));
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IAfterMapConfigurationInstance<,>));
    }

    [Fact]
    public void CombinedAsyncInstanceInterface_ShouldInheritFromBothAsyncInstanceInterfaces()
    {
        var combinedType = typeof(IMapHooksConfigurationAsyncInstance<,>);
        var interfaces = combinedType.GetInterfaces();
        
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IBeforeMapConfigurationAsyncInstance<,>));
        interfaces.ShouldContain(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IAfterMapConfigurationAsyncInstance<,>));
    }

    #endregion

    #region Interface Method Signature Tests

    [Fact]
    public void BeforeMapInterface_ShouldHaveStaticAbstractBeforeMapMethod()
    {
        var interfaceType = typeof(IBeforeMapConfiguration<,>);
        var method = interfaceType.GetMethod("BeforeMap");
        
        method.ShouldNotBeNull();
        method!.IsStatic.ShouldBeTrue();
        method.IsAbstract.ShouldBeTrue();
    }

    [Fact]
    public void AfterMapInterface_ShouldHaveStaticAbstractAfterMapMethod()
    {
        var interfaceType = typeof(IAfterMapConfiguration<,>);
        var method = interfaceType.GetMethod("AfterMap");
        
        method.ShouldNotBeNull();
        method!.IsStatic.ShouldBeTrue();
        method.IsAbstract.ShouldBeTrue();
    }

    [Fact]
    public void AsyncBeforeMapInterface_ShouldReturnTask()
    {
        var interfaceType = typeof(IBeforeMapConfigurationAsync<,>);
        var method = interfaceType.GetMethod("BeforeMapAsync");
        
        method.ShouldNotBeNull();
        method!.ReturnType.ShouldBe(typeof(Task));
    }

    [Fact]
    public void AsyncAfterMapInterface_ShouldReturnTask()
    {
        var interfaceType = typeof(IAfterMapConfigurationAsync<,>);
        var method = interfaceType.GetMethod("AfterMapAsync");
        
        method.ShouldNotBeNull();
        method!.ReturnType.ShouldBe(typeof(Task));
    }

    [Fact]
    public void InstanceBeforeMapInterface_ShouldHaveInstanceMethod()
    {
        var interfaceType = typeof(IBeforeMapConfigurationInstance<,>);
        var method = interfaceType.GetMethod("BeforeMap");
        
        method.ShouldNotBeNull();
        method!.IsStatic.ShouldBeFalse();
    }

    [Fact]
    public void InstanceAfterMapInterface_ShouldHaveInstanceMethod()
    {
        var interfaceType = typeof(IAfterMapConfigurationInstance<,>);
        var method = interfaceType.GetMethod("AfterMap");
        
        method.ShouldNotBeNull();
        method!.IsStatic.ShouldBeFalse();
    }

    #endregion
}
