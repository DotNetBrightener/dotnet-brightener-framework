using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp.CommonShared.Endpoints.Validation;
using Xunit;

namespace WebApp.CommonShared.Tests;

/// <summary>
///     Unit tests for ModelValidationExtensions
/// </summary>
public class ModelValidationExtensionsTests
{
    [Fact]
    public void Validate_WithNoRegisteredValidator_ShouldReturnValidResult()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };
        var context = CreateHttpContextWithoutValidator();

        // Act
        var result = context.Validate(model);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithValidModel_ShouldReturnValidResult()
    {
        // Arrange
        var model = new TestModel { Name = "Valid Name" };
        var context = CreateHttpContextWithValidator();

        // Act
        var result = context.Validate(model);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithInvalidModel_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new TestModel { Name = "" };
        var context = CreateHttpContextWithValidator();

        // Act
        var result = context.Validate(model);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ValidateAsync_WithValidModel_ShouldReturnValidResult()
    {
        // Arrange
        var model = new TestModel { Name = "Valid Name" };
        var context = CreateHttpContextWithValidator();

        // Act
        var result = await context.ValidateAsync(model);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidModel_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new TestModel { Name = "" };
        var context = CreateHttpContextWithValidator();

        // Act
        var result = await context.ValidateAsync(model);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ValidateAndGetResult_WithValidModel_ShouldReturnNull()
    {
        // Arrange
        var model = new TestModel { Name = "Valid Name" };
        var context = CreateHttpContextWithValidator();

        // Act
        var result = context.ValidateAndGetResult(model);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ValidateAndGetResult_WithInvalidModel_ShouldReturnValidationProblem()
    {
        // Arrange
        var model = new TestModel { Name = "" };
        var context = CreateHttpContextWithValidator();

        // Act
        var result = context.ValidateAndGetResult(model);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void ToDictionary_WithValidationErrors_ShouldGroupByPropertyName()
    {
        // Arrange
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Name", "Name must be at least 3 characters"),
            new ValidationFailure("Email", "Invalid email format")
        });

        // Act
        var dictionary = validationResult.ToDictionary();

        // Assert
        dictionary.Count.ShouldBe(2);
        dictionary["Name"].Length.ShouldBe(2);
        dictionary["Email"].Length.ShouldBe(1);
    }

    #region Helper Methods

    private static HttpContext CreateHttpContextWithoutValidator()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        return context;
    }

    private static HttpContext CreateHttpContextWithValidator()
    {
        var services = new ServiceCollection();
        services.AddTransient<IValidator<TestModel>, TestModelValidator>();
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        return context;
    }

    #endregion

    #region Test Models

    public class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestModelValidator : AbstractValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters");
        }
    }

    #endregion
}
