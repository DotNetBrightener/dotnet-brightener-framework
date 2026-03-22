using System.Linq.Expressions;
using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions.Mapping;

public class ExpressionMappingTests
{
    private static List<User> CreateTestUsers()
    {
        var users = new List<User>
        {
            TestDataFactory.CreateUser("John", "Doe"),
            TestDataFactory.CreateUser("Jane", "Smith"),
            TestDataFactory.CreateUser("Bob", "Johnson"),
            TestDataFactory.CreateUser("Alice", "Williams")
        };
        
        // Set predictable IDs for testing
        for (int i = 0; i < users.Count; i++)
        {
            users[i].Id = i + 1; // IDs will be 1, 2, 3, 4
        }
        
        return users;
    }

    private static List<UserDto> CreateTestUserDtos()
    {
        var users = CreateTestUsers();
        return users.Select(u => u.ToTarget<User, UserDto>()).ToList();
    }

    [Fact]
    public void MapToTarget_ShouldTransformSimplePredicate()
    {
        // Arrange
        Expression<Func<User, bool>> sourcePredicate = u => u.Id > 1;

        // Act
        Expression<Func<UserDto, bool>> targetPredicate = sourcePredicate.MapToTarget<UserDto>();
        var compiledPredicate = targetPredicate.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledPredicate).ToList();

        // Should return users with Id > 1
        results.Count().ShouldBe(3);
        results.ShouldNotContain(dto => dto.Id == 1);
    }

    [Fact]
    public void MapToTarget_ShouldTransformComplexPredicate()
    {
        // Arrange
        Expression<Func<User, bool>> sourcePredicate = u => 
            u.Id > 1 && u.FirstName.StartsWith("J");

        // Act
        Expression<Func<UserDto, bool>> targetPredicate = sourcePredicate.MapToTarget<UserDto>();
        var compiledPredicate = targetPredicate.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledPredicate).ToList();

        // Should match users with Id > 1 and FirstName starts with "J" (Jane has Id=2)
        results.ShouldAllBe(dto => 
            dto.Id > 1 && 
            dto.FirstName.StartsWith("J"));
        results.Count().ShouldBe(1); // Jane Smith should match
        results[0].FirstName.ShouldBe("Jane");
    }

    [Fact]
    public void MapToTarget_ShouldHandleComplexPredicateCorrectly()
    {
        // Arrange
        Expression<Func<User, bool>> sourcePredicate = u => 
            u.IsActive && u.FirstName.StartsWith("J");

        // Act
        Expression<Func<UserDto, bool>> targetPredicate = sourcePredicate.MapToTarget<UserDto>();
        var compiledPredicate = targetPredicate.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledPredicate).ToList();

        // Should match active users whose first name starts with "J"
        results.ShouldAllBe(dto => dto.IsActive && dto.FirstName.StartsWith("J"));
    }

    [Fact]
    public void MapToTarget_ShouldTransformLogicalOperators()
    {
        // Arrange
        Expression<Func<User, bool>> sourcePredicate = u => 
            u.FirstName == "John" || u.FirstName == "Jane";

        // Act
        Expression<Func<UserDto, bool>> targetPredicate = sourcePredicate.MapToTarget<UserDto>();
        var compiledPredicate = targetPredicate.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledPredicate).ToList();

        results.Count().ShouldBeGreaterThanOrEqualTo(1);
        results.ShouldAllBe(dto => dto.FirstName == "John" || dto.FirstName == "Jane");
    }

    [Fact]
    public void MapToTarget_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<User, bool>> nullPredicate = null;

        // Act & Assert
        var act = () => nullPredicate.MapToTarget<UserDto>();
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void MapToTarget_ShouldTransformSelector()
    {
        // Arrange
        Expression<Func<User, string>> sourceSelector = u => u.LastName;

        // Act
        Expression<Func<UserDto, string>> targetSelector = sourceSelector.MapToTarget<UserDto, string>();
        var compiledSelector = targetSelector.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Select(compiledSelector).ToList();

        results.Count().ShouldBe(4);
        results.ShouldContain("Doe");
        results.ShouldContain("Smith");
        results.ShouldContain("Johnson");
        results.ShouldContain("Williams");
    }

    [Fact]
    public void MapToTarget_ShouldTransformIntSelector()
    {
        // Arrange
        Expression<Func<User, int>> sourceSelector = u => u.Id;

        // Act
        Expression<Func<UserDto, int>> targetSelector = sourceSelector.MapToTarget<UserDto, int>();
        var compiledSelector = targetSelector.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Select(compiledSelector).OrderBy(x => x).ToList();

        // Should return ordered IDs
        results.Count().ShouldBeGreaterThan(0);
        results.ShouldBeInOrder(Shouldly.SortDirection.Ascending);
    }

    [Fact]
    public void MapToTarget_ShouldHandleSelectorWithMethodCall()
    {
        // Arrange
        Expression<Func<User, string>> sourceSelector = u => u.FirstName.ToUpper();

        // Act
        Expression<Func<UserDto, string>> targetSelector = sourceSelector.MapToTarget<UserDto, string>();
        var compiledSelector = targetSelector.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Select(compiledSelector).ToList();

        results.ShouldContain("JOHN");
        results.ShouldContain("JANE");
        results.ShouldContain("BOB");
        results.ShouldContain("ALICE");
    }

    [Fact]
    public void MapToTargetGeneric_ShouldTransformLambdaExpression()
    {
        // Arrange
        Expression<Func<User, object>> sourceExpression = u => new { u.FirstName, u.Id };

        // Act
        var targetExpression = sourceExpression.MapToTargetGeneric<UserDto>();

        // Assert
        targetExpression.ShouldNotBeNull();
        targetExpression.Parameters.Count().ShouldBe(1);
        targetExpression.Parameters[0].Type.ShouldBe(typeof(UserDto));
    }

    [Fact]
    public void MapToTargetGeneric_ShouldPreserveExpressionStructure()
    {
        // Arrange
        Expression<Func<User, bool>> sourceExpression = u => u.Id > 1 && u.FirstName != null;

        // Act
        var targetExpression = sourceExpression.MapToTargetGeneric<UserDto>();

        // Assert
        targetExpression.ShouldNotBeNull();
        targetExpression.Parameters[0].Type.ShouldBe(typeof(UserDto));
        
        // Verify the expression can be compiled and executed
        var compiledExpression = (Expression<Func<UserDto, bool>>)targetExpression;
        var compiled = compiledExpression.Compile();
        
        var testDto = CreateTestUserDtos().First();
        var result = compiled(testDto);
        // Just verify we can execute the compiled expression without error
        (result is bool).ShouldBeTrue();
    }

    [Fact]
    public void CombineWithAnd_ShouldCombineMultiplePredicates()
    {
        // Arrange
        var hasValidId = (Expression<Func<User, bool>>)(u => u.Id > 0);
        var hasValidEmail = (Expression<Func<User, bool>>)(u => !string.IsNullOrEmpty(u.Email));
        var isFirstNameNotEmpty = (Expression<Func<User, bool>>)(u => !string.IsNullOrEmpty(u.FirstName));

        // Act
        var combinedPredicate = MappingExpressionExtensions.CombineWithAnd(
            hasValidId, hasValidEmail, isFirstNameNotEmpty);

        // Assert
        combinedPredicate.ShouldNotBeNull();
        var compiled = combinedPredicate.Compile();
        
        var testUsers = CreateTestUsers();
        var results = testUsers.Where(compiled).ToList();
        
        results.Count().ShouldBe(4); // All test users should match these basic conditions
    }

    [Fact]
    public void CombineWithOr_ShouldCombineMultiplePredicates()
    {
        // Arrange
        var firstNameStartsWithA = (Expression<Func<User, bool>>)(u => u.FirstName.StartsWith("A"));
        var firstNameStartsWithJ = (Expression<Func<User, bool>>)(u => u.FirstName.StartsWith("J"));

        // Act
        var combinedPredicate = MappingExpressionExtensions.CombineWithOr(firstNameStartsWithA, firstNameStartsWithJ);

        // Assert
        combinedPredicate.ShouldNotBeNull();
        var compiled = combinedPredicate.Compile();
        
        var testUsers = CreateTestUsers();
        var results = testUsers.Where(compiled).ToList();
        
        // Should match users whose first name starts with A or J
        results.ShouldAllBe(u => u.FirstName.StartsWith("A") || u.FirstName.StartsWith("J"));
    }

    [Fact]
    public void CombineWithAnd_WithEmptyArray_ShouldReturnAlwaysTrue()
    {
        // Act
        var result = MappingExpressionExtensions.CombineWithAnd<User>();

        // Assert
        var compiled = result.Compile();
        var testUser = CreateTestUsers().First();
        compiled(testUser).ShouldBeTrue();
    }

    [Fact]
    public void CombineWithOr_WithEmptyArray_ShouldReturnAlwaysFalse()
    {
        // Act
        var result = MappingExpressionExtensions.CombineWithOr<User>();

        // Assert
        var compiled = result.Compile();
        var testUser = CreateTestUsers().First();
        compiled(testUser).ShouldBeFalse();
    }

    [Fact]
    public void CombineWithAnd_WithSinglePredicate_ShouldReturnSamePredicate()
    {
        // Arrange
        var predicate = (Expression<Func<User, bool>>)(u => u.Id > 1);

        // Act
        var result = MappingExpressionExtensions.CombineWithAnd(predicate);

        // Assert
        result.ShouldBeSameAs(predicate);
    }

    [Fact]
    public void Negate_ShouldCreateOppositeCondition()
    {
        // Arrange
        var originalPredicate = (Expression<Func<User, bool>>)(u => u.IsActive);

        // Act
        var negatedPredicate = originalPredicate.Negate();

        // Assert
        var compiledOriginal = originalPredicate.Compile();
        var compiledNegated = negatedPredicate.Compile();
        
        var testUsers = CreateTestUsers();
        
        foreach (var user in testUsers)
        {
            // The negated predicate should give opposite results
            compiledOriginal(user).ShouldBe(!compiledNegated(user));
        }
    }

    [Fact]
    public void Negate_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<User, bool>> nullPredicate = null;

        // Act & Assert
        var act = () => nullPredicate.Negate();
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void IntegrationTest_PredicateMappingWithComposition()
    {
        // Arrange - Create complex business logic for entities
        var isActiveUser = (Expression<Func<User, bool>>)(u => u.IsActive);
        var hasValidId = (Expression<Func<User, bool>>)(u => u.Id > 0);
        var hasValidName = (Expression<Func<User, bool>>)(u => 
            !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName));

        // Combine business rules
        var validUserFilter = MappingExpressionExtensions.CombineWithAnd(
            isActiveUser, hasValidId, hasValidName);

        // Act - Transform to work with DTOs
        var dtoFilter = validUserFilter.MapToTarget<UserDto>();
        var compiledDtoFilter = dtoFilter.Compile();

        // Assert - Verify it works with DTOs
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledDtoFilter).ToList();

        // Should only contain active users with valid data
        results.ShouldAllBe(dto => !string.IsNullOrEmpty(dto.FirstName) && !string.IsNullOrEmpty(dto.LastName));
    }

    [Fact]
    public void IntegrationTest_SelectorMappingWithSorting()
    {
        // Arrange
        var idSelector = (Expression<Func<User, int>>)(u => u.Id);
        var nameSelector = (Expression<Func<User, string>>)(u => u.LastName);

        // Act - Transform selectors to work with DTOs
        var dtoIdSelector = idSelector.MapToTarget<UserDto, int>();
        var dtoNameSelector = nameSelector.MapToTarget<UserDto, string>();

        var compiledIdSelector = dtoIdSelector.Compile();
        var compiledNameSelector = dtoNameSelector.Compile();

        // Assert - Use for sorting DTOs
        var testDtos = CreateTestUserDtos();
        
        var sortedById = testDtos.OrderBy(compiledIdSelector).ToList();
        var sortedByName = testDtos.OrderBy(compiledNameSelector).ToList();

        // Verify ID sorting
        sortedById.Select(dto => dto.Id).ShouldBeInOrder(Shouldly.SortDirection.Ascending);

        // Verify name sorting
        sortedByName.Select(dto => dto.LastName).ShouldBeInOrder(Shouldly.SortDirection.Ascending);
    }

    [Fact]
    public void IntegrationTest_ComplexExpressionTransformation()
    {
        // Arrange - Create a complex business rule expression
        Expression<Func<User, bool>> complexBusinessRule = u =>
            u.Id > 0 &&
            (u.FirstName.StartsWith("J") || u.LastName.Contains("son")) &&
            u.IsActive;

        // Act - Transform to DTO
        var dtoBusinessRule = complexBusinessRule.MapToTarget<UserDto>();
        var compiledDtoRule = dtoBusinessRule.Compile();

        // Assert - Verify complex logic works with DTOs
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledDtoRule).ToList();

        // Should match users with valid IDs, names starting with J or containing "son", and are active
        results.ShouldAllBe(dto => 
            dto.Id > 0 && 
            (dto.FirstName.StartsWith("J") || dto.LastName.Contains("son")) &&
            dto.IsActive);
    }

    [Fact]
    public void MapToTarget_WithNullArguments_ShouldThrowArgumentNullException()
    {
        // Test predicate mapping with null
        Expression<Func<User, bool>> nullPredicate = null;
        var act1 = () => nullPredicate.MapToTarget<UserDto>();
        act1.ShouldThrow<ArgumentNullException>();

        // Test selector mapping with null
        Expression<Func<User, string>> nullSelector = null;
        var act2 = () => nullSelector.MapToTarget<UserDto, string>();
        act2.ShouldThrow<ArgumentNullException>();

        // Test generic mapping with null
        LambdaExpression nullExpression = null;
        var act3 = () => nullExpression.MapToTargetGeneric<UserDto>();
        act3.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void CombineWithAnd_WithNullArray_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => MappingExpressionExtensions.CombineWithAnd<User>(null);
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void CombineWithOr_WithNullArray_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => MappingExpressionExtensions.CombineWithOr<User>(null);
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void MapToTarget_ShouldHandlePropertyThatExistsInBothTypes()
    {
        // Arrange - Test properties that exist in both User and UserDto
        Expression<Func<User, bool>> predicate = u => u.Id > 0 && !string.IsNullOrEmpty(u.FirstName);

        // Act
        var dtoFilter = predicate.MapToTarget<UserDto>();
        var compiledFilter = dtoFilter.Compile();

        // Assert
        var testDtos = CreateTestUserDtos();
        var results = testDtos.Where(compiledFilter).ToList();
        
        // Should return all users since they all have valid IDs and names
        results.Count().ShouldBeGreaterThan(0);
        results.ShouldAllBe(dto => dto.Id > 0 && !string.IsNullOrEmpty(dto.FirstName));
    }
}
