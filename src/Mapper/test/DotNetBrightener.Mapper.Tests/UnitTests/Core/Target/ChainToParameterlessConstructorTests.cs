using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     When a user has initialization logic in their parameterless constructor,
///     the generated constructor should be able to chain to it using `: this()`.
/// </summary>
public class ChainToParameterlessConstructorTests
{
    [Fact]
    public void ChainedConstructor_ShouldCallParameterlessConstructorFirst()
    {
        // Arrange
        var source = new ModelTypeForChaining
        {
            MaxValue = 42,
            Name = "Test"
        };

        // Act
        var dto = new ChainedConstructorDto(source);

        // Assert - Parameterless constructor should have run, setting Value = 100 and Initialized = true
        // Then the source properties should be mapped
        dto.Initialized.ShouldBeTrue("Parameterless constructor should have run");
        dto.Value.ShouldBe(100, "Value should be initialized by parameterless constructor");
        dto.MaxValue.ShouldBe(42, "MaxValue should be mapped from source");
        dto.Name.ShouldBe("Test", "Name should be mapped from source");
    }

    [Fact]
    public void NonChainedConstructor_ShouldNotCallParameterlessConstructor()
    {
        // Arrange
        var source = new ModelTypeForChaining
        {
            MaxValue = 42,
            Name = "Test"
        };

        // Act
        var dto = new NonChainedConstructorDto(source);

        // Assert - Parameterless constructor should NOT have run
        // The Initialized and Value properties should have their default values
        dto.Initialized.ShouldBeFalse("Parameterless constructor should NOT have run");
        dto.Value.ShouldBe(0, "Value should have default value since parameterless ctor didn't run");
        dto.MaxValue.ShouldBe(42, "MaxValue should still be mapped from source");
        dto.Name.ShouldBe("Test", "Name should still be mapped from source");
    }

    [Fact]
    public void ChainedConstructorNoDepth_ShouldCallParameterlessConstructor()
    {
        // Arrange
        var source = new ModelTypeForChaining
        {
            MaxValue = 99,
            Name = "NoDepth"
        };

        // Act
        var dto = new ChainedConstructorNoDepthDto(source);

        // Assert - Parameterless constructor should have run with different Value
        dto.Initialized.ShouldBeTrue("Parameterless constructor should have run");
        dto.Value.ShouldBe(200, "Value should be 200 from this specific parameterless constructor");
        dto.MaxValue.ShouldBe(99, "MaxValue should be mapped from source");
        dto.Name.ShouldBe("NoDepth", "Name should be mapped from source");
    }

    [Fact]
    public void ChainedConstructor_ManualParameterlessCall_ShouldWork()
    {
        // Arrange & Act - Call the user's parameterless constructor directly
        var dto = new ChainedConstructorDto();

        // Assert
        dto.Initialized.ShouldBeTrue();
        dto.Value.ShouldBe(100);
        dto.MaxValue.ShouldBe(0, "MaxValue should have default int value");
        // Note: Name won't be set by parameterless constructor, but the property has a default value from the source
    }

    [Fact]
    public void ChainedConstructor_FromSource_ShouldWorkWithChaining()
    {
        // Arrange
        var source = new ModelTypeForChaining
        {
            MaxValue = 123,
            Name = "FromSource"
        };

        // Act
        var dto = ChainedConstructorDto.FromSource(source);

        // Assert - FromSource should also use the constructor that chains
        dto.Initialized.ShouldBeTrue("Initialization should have occurred");
        dto.Value.ShouldBe(100, "Value should be set by parameterless constructor");
        dto.MaxValue.ShouldBe(123, "MaxValue should be mapped from source");
    }
}
