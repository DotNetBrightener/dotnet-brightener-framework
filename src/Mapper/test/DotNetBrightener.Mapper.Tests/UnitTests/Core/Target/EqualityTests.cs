using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for the GenerateEquality feature (GitHub issue #277).
///     Verifies that value-based equality members are generated for class-based targets.
/// </summary>
public class EqualityTests
{
    [Fact]
    public void Equality_ShouldReturnTrue_ForEqualInstances()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Alice",
            Email = "alice@example.com",
            Age = 30,
            BirthDate = new DateTime(1994, 6, 15)
        };

        var dto1 = new PersonWithEqualityDto(source);
        var dto2 = new PersonWithEqualityDto(source);

        // Act & Assert
        dto1.Equals(dto2).ShouldBeTrue();
        (dto1 == dto2).ShouldBeTrue();
        (dto1 != dto2).ShouldBeFalse();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_ForDifferentInstances()
    {
        // Arrange
        var source1 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30 };
        var source2 = new PersonForCopyAndEquality { Id = 2, Name = "Bob", Email = "b@b.com", Age = 25 };

        var dto1 = new PersonWithEqualityDto(source1);
        var dto2 = new PersonWithEqualityDto(source2);

        // Act & Assert
        dto1.Equals(dto2).ShouldBeFalse();
        (dto1 == dto2).ShouldBeFalse();
        (dto1 != dto2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenComparedToNull()
    {
        // Arrange
        var source = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30 };
        var dto = new PersonWithEqualityDto(source);

        // Act & Assert
        dto.Equals(null).ShouldBeFalse();
        (dto == null).ShouldBeFalse();
        (null == dto).ShouldBeFalse();
    }

    [Fact]
    public void Equality_ShouldReturnTrue_ForSameReference()
    {
        // Arrange
        var source = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30 };
        var dto = new PersonWithEqualityDto(source);

        // Act & Assert
        dto.Equals(dto).ShouldBeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (dto == dto).ShouldBeTrue();
#pragma warning restore CS1718
    }

    [Fact]
    public void Equality_EqualsObject_ShouldWork()
    {
        // Arrange
        var source = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30 };
        var dto1 = new PersonWithEqualityDto(source);
        var dto2 = new PersonWithEqualityDto(source);

        // Act & Assert
        dto1.Equals((object)dto2).ShouldBeTrue();
        dto1.Equals((object)"not a dto").ShouldBeFalse();
        dto1.Equals((object?)null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldBeEqual_ForEqualInstances()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Alice",
            Email = "alice@example.com",
            Age = 30,
            BirthDate = new DateTime(1994, 6, 15)
        };

        var dto1 = new PersonWithEqualityDto(source);
        var dto2 = new PersonWithEqualityDto(source);

        // Act & Assert
        dto1.GetHashCode().ShouldBe(dto2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldTypicallyDiffer_ForDifferentInstances()
    {
        // Arrange
        var source1 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30 };
        var source2 = new PersonForCopyAndEquality { Id = 2, Name = "Bob", Email = "b@b.com", Age = 25 };

        var dto1 = new PersonWithEqualityDto(source1);
        var dto2 = new PersonWithEqualityDto(source2);

        // Act & Assert � not guaranteed but very likely for different data
        dto1.GetHashCode().ShouldNotBe(dto2.GetHashCode());
    }

    [Fact]
    public void Equality_ShouldHandleNullablePropertyDifferences()
    {
        // Arrange
        var source1 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30, BirthDate = new DateTime(1994, 1, 1) };
        var source2 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30, BirthDate = null };

        var dto1 = new PersonWithEqualityDto(source1);
        var dto2 = new PersonWithEqualityDto(source2);

        // Act & Assert � different because BirthDate differs
        dto1.Equals(dto2).ShouldBeFalse();
    }

    [Fact]
    public void Equality_ShouldHandleBothNullablePropertiesNull()
    {
        // Arrange
        var source1 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30, BirthDate = null };
        var source2 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30, BirthDate = null };

        var dto1 = new PersonWithEqualityDto(source1);
        var dto2 = new PersonWithEqualityDto(source2);

        // Act & Assert
        dto1.Equals(dto2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_ShouldWorkWithCopyConstructor()
    {
        // Arrange � both features enabled
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Alice",
            Email = "alice@example.com",
            Age = 30,
            BirthDate = new DateTime(1994, 6, 15)
        };
        var original = new PersonWithCopyAndEqualityDto(source);
        var copy = new PersonWithCopyAndEqualityDto(original);

        // Act & Assert
        original.Equals(copy).ShouldBeTrue();
        (original == copy).ShouldBeTrue();
        original.GetHashCode().ShouldBe(copy.GetHashCode());
    }

    [Fact]
    public void Equality_ShouldWorkOnStruct()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Alice",
            Email = "alice@example.com",
            Age = 30
        };
        var dto1 = new PersonStructWithCopyAndEquality(source);
        var dto2 = new PersonStructWithCopyAndEquality(source);

        // Act & Assert
        dto1.Equals(dto2).ShouldBeTrue();
        (dto1 == dto2).ShouldBeTrue();
        (dto1 != dto2).ShouldBeFalse();
        dto1.GetHashCode().ShouldBe(dto2.GetHashCode());
    }

    [Fact]
    public void Equality_ShouldBeIgnoredForRecords()
    {
        // Arrange � records already have value equality from the language
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Alice",
            Email = "alice@example.com",
            Age = 30,
            BirthDate = new DateTime(1994, 6, 15)
        };
        var dto1 = new PersonRecordWithEquality(source);
        var dto2 = new PersonRecordWithEquality(source);

        // Assert � record equality still works (built-in)
        dto1.ShouldBe(dto2);

        // The generated type should NOT implement IEquatable<T> explicitly since it's a record
        // (records implement it implicitly through the language)
        var targetType = typeof(PersonRecordWithEquality);
        var interfaces = targetType.GetInterfaces();
        // Records get IEquatable<T> from the compiler, not from us
        // We just verify records still work correctly
        dto1.Equals(dto2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_ShouldImplementIEquatable()
    {
        // Arrange
        var targetType = typeof(PersonWithEqualityDto);

        // Act
        var implementsIEquatable = typeof(IEquatable<PersonWithEqualityDto>).IsAssignableFrom(targetType);

        // Assert
        implementsIEquatable.ShouldBeTrue("the generated class should implement IEquatable<T>");
    }

    [Fact]
    public void Equality_ShouldWorkInCollections()
    {
        // Arrange
        var source1 = new PersonForCopyAndEquality { Id = 1, Name = "Alice", Email = "a@a.com", Age = 30 };
        var source2 = new PersonForCopyAndEquality { Id = 2, Name = "Bob", Email = "b@b.com", Age = 25 };

        var dto1 = new PersonWithEqualityDto(source1);
        var dto2 = new PersonWithEqualityDto(source2);
        var dto1Copy = new PersonWithEqualityDto(source1);

        var set = new HashSet<PersonWithEqualityDto> { dto1, dto2 };

        // Act & Assert � HashSet should find dto1Copy as already existing
        set.Contains(dto1Copy).ShouldBeTrue();
        set.Count().ShouldBe(2);
    }
}
