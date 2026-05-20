using System.Linq.Expressions;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests to verify the structure of generated projection expressions,
///     particularly for EF Core compatibility with nested targets.
/// </summary>
public class ProjectionStructureTests
{
    [Fact]
    public void Projection_ShouldUseObjectInitializer_NotConstructor()
    {
        // Arrange & Act
        var projection = CompanyTarget.Projection;

        // Assert
        projection.ShouldNotBeNull();

        // The projection should be a lambda expression
        var body = projection.Body;
        body.ShouldBeOfType<MemberInitExpression>(
            "EF Core can translate MemberInitExpression (object initializer) but not constructor calls");

        var memberInit = (MemberInitExpression)body;

        // Verify it's initializing properties, not calling a constructor with parameters
        memberInit.NewExpression.Arguments.Count.ShouldBe(0,
            "Object initializer should use parameterless constructor");

        // Verify it has member bindings for properties
        memberInit.Bindings.ShouldNotBeEmpty("Should have property assignments");
    }

    [Fact]
    public void Projection_WithNestedTarget_ShouldAccessNavigationPropertyMembers()
    {
        // Arrange & Act
        var projection = CompanyTarget.Projection;

        // Assert
        var body = (MemberInitExpression)projection.Body;

        // Find the HeadquartersAddress binding
        var addressBinding = body.Bindings
            .OfType<MemberAssignment>()
            .FirstOrDefault(b => b.Member.Name == "HeadquartersAddress");

        addressBinding.ShouldNotBeNull("Should have HeadquartersAddress property assignment");

        // The expression should access source.HeadquartersAddress
        // This is critical for EF Core to know to load the navigation property
        var addressExpression = addressBinding!.Expression.ToString();
        addressExpression.ShouldContain("source.HeadquartersAddress");
    }

    [Fact]
    public void Projection_WithCollectionNestedTarget_ShouldAccessCollectionMembers()
    {
        // Arrange & Act
        var projection = OrderTarget.Projection;

        // Assert
        var body = (MemberInitExpression)projection.Body;

        // Find the Items collection binding
        var itemsBinding = body.Bindings
            .OfType<MemberAssignment>()
            .FirstOrDefault(b => b.Member.Name == "Items");

        itemsBinding.ShouldNotBeNull("Should have Items collection property assignment");

        // The expression should use Select on source.Items
        var itemsExpression = itemsBinding!.Expression.ToString();
        itemsExpression.ShouldContain("source.Items");
        itemsExpression.ShouldContain("Select");
    }

    [Fact]
    public void Projection_ToString_ShouldShowObjectInitializerSyntax()
    {
        // Arrange & Act
        var projection = CompanyTarget.Projection;
        var projectionString = projection.ToString();

        // Assert
        // Should see "source => new CompanyTarget { ... }"
        // NOT "source => new CompanyTarget(source)"
        projectionString.ShouldNotContain("new CompanyTarget(source)");
    }

    [Fact]
    public void Projection_WithNullableNestedTarget_ShouldHaveNullCheck()
    {
        // Arrange & Act
        var projection = DataTableTargetDto.Projection;

        // Assert
        var body = (MemberInitExpression)projection.Body;

        // Find the ExtendedData binding (nullable nested target)
        var extendedDataBinding = body.Bindings
            .OfType<MemberAssignment>()
            .FirstOrDefault(b => b.Member.Name == "ExtendedData");

        extendedDataBinding.ShouldNotBeNull("Should have ExtendedData property assignment");

        // The expression should have a conditional for null check
        var expression = extendedDataBinding!.Expression;
        expression.NodeType.ShouldBe(ExpressionType.Conditional,
            "Nullable nested target should use conditional expression (ternary operator)");
    }
}
