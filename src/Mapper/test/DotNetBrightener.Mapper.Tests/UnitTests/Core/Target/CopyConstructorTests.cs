using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for the GenerateCopyConstructor feature
///     Verifies that a copy constructor is generated that copies all member values from another instance.
/// </summary>
public class CopyConstructorTests
{
    [Fact]
    public void CopyConstructor_ShouldCopyAllProperties()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 42,
            Name = "Alice",
            Email = "alice@example.com",
            Age = 30,
            BirthDate = new DateTime(1994, 6, 15)
        };
        var original = new PersonWithCopyConstructorDto(source);

        // Act
        var copy = new PersonWithCopyConstructorDto(original);

        // Assert
        copy.ShouldNotBeSameAs(original);
        copy.Id.ShouldBe(42);
        copy.Name.ShouldBe("Alice");
        copy.Email.ShouldBe("alice@example.com");
        copy.Age.ShouldBe(30);
        copy.BirthDate.ShouldBe(new DateTime(1994, 6, 15));
    }

    [Fact]
    public void CopyConstructor_ShouldHandleNullableProperties()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Bob",
            Email = "bob@example.com",
            Age = 25,
            BirthDate = null
        };
        var original = new PersonWithCopyConstructorDto(source);

        // Act
        var copy = new PersonWithCopyConstructorDto(original);

        // Assert
        copy.BirthDate.ShouldBeNull();
    }

    [Fact]
    public void CopyConstructor_ShouldCreateIndependentCopy()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 1,
            Name = "Charlie",
            Email = "charlie@example.com",
            Age = 35,
            BirthDate = new DateTime(1989, 1, 1)
        };
        var original = new PersonWithCopyConstructorDto(source);

        // Act
        var copy = new PersonWithCopyConstructorDto(original);
        // Modify the original � the copy should remain unchanged
        original.Name = "Changed";
        original.Age = 99;

        // Assert
        copy.Name.ShouldBe("Charlie");
        copy.Age.ShouldBe(35);
    }

    [Fact]
    public void CopyConstructor_ShouldThrowOnNull_ForClassTargets()
    {
        // Act & Assert
        var act = () => new PersonWithCopyConstructorDto((PersonWithCopyConstructorDto)null!);
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void CopyConstructor_ShouldWorkWithBothFeatures()
    {
        // Arrange � target with both GenerateCopyConstructor and GenerateEquality
        var source = new PersonForCopyAndEquality
        {
            Id = 7,
            Name = "Diana",
            Email = "diana@example.com",
            Age = 28,
            BirthDate = new DateTime(1996, 3, 20)
        };
        var original = new PersonWithCopyAndEqualityDto(source);

        // Act
        var copy = new PersonWithCopyAndEqualityDto(original);

        // Assert � copy should equal the original
        copy.ShouldBe(original);
        copy.Id.ShouldBe(7);
    }

    [Fact]
    public void CopyConstructor_ShouldWorkOnStruct()
    {
        // Arrange
        var source = new PersonForCopyAndEquality
        {
            Id = 10,
            Name = "Eve",
            Email = "eve@example.com",
            Age = 22
        };
        var original = new PersonStructWithCopyAndEquality(source);

        // Act
        var copy = new PersonStructWithCopyAndEquality(original);

        // Assert
        copy.Id.ShouldBe(10);
        copy.Name.ShouldBe("Eve");
        copy.Email.ShouldBe("eve@example.com");
        copy.Age.ShouldBe(22);
    }
}
