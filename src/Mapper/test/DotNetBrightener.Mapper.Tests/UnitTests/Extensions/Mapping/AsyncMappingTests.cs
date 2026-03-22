using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions.Mapping;

public class AsyncMappingTests
{
    [Fact]
    public async Task ToTargetAsync_ShouldMapSingleInstance()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com", new DateTime(1990, 1, 1));

        // Act
        var result = await user.ToTargetAsync<UserDto, UserDtoAsyncMapper>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.FirstName.ShouldBe("John");
        result.LastName.ShouldBe("Doe");
        result.Email.ShouldBe("john@example.com");
        result.FullName.ShouldBe("John Doe");
        result.Age.ShouldBeGreaterThan(30);
    }

    [Fact]
    public async Task ToTargetAsync_ShouldHandleCancellation()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var act = () => user.ToTargetAsync<UserDto, UserDtoAsyncMapper>(cts.Token);
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ToTargetAsync_ShouldCalculateAgeCorrectly()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-25);
        var user = TestDataFactory.CreateUser("Jane", "Smith", dateOfBirth: birthDate);

        // Act
        var result = await user.ToTargetAsync<UserDto, UserDtoAsyncMapper>();

        // Assert
        result.Age.ShouldBe(25);
        result.FullName.ShouldBe("Jane Smith");
    }

    [Fact]
    public async Task ToTargetAsync_ShouldWorkWithDifferentSourceTypes()
    {
        // Arrange
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            Price = 99.99m, 
            CategoryId = 5,
            IsAvailable = true
        };

        // Act
        var result = await product.ToTargetAsync<ProductDto, ProductDtoAsyncMapper>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Name.ShouldBe("Test Product");
        result.Price.ShouldBe(99.99m);
    }

    [Fact]
    public async Task ToTargetsAsync_ShouldMapCollection()
    {
        // Arrange
        var users = new List<User>
        {
            TestDataFactory.CreateUser("John", "Doe", "john@example.com"),
            TestDataFactory.CreateUser("Jane", "Smith", "jane@example.com"),
            TestDataFactory.CreateUser("Bob", "Johnson", "bob@example.com")
        };

        // Act
        var results = await users.ToTargetsAsync<UserDto, UserDtoAsyncMapper>();

        // Assert
        results.ShouldNotBeNull();
        results.Count().ShouldBe(3);
        
        var first = results[0];
        first.FirstName.ShouldBe("John");
        first.LastName.ShouldBe("Doe");
        first.Email.ShouldBe("john@example.com");
        first.FullName.ShouldBe("John Doe");

        var second = results[1];
        second.FirstName.ShouldBe("Jane");
        second.FullName.ShouldBe("Jane Smith");
        
        var third = results[2];
        third.FirstName.ShouldBe("Bob");
        third.FullName.ShouldBe("Bob Johnson");
    }

    [Fact]
    public async Task ToTargetsAsync_ShouldHandleEmptyCollection()
    {
        // Arrange
        var emptyUsers = new List<User>();

        // Act
        var results = await emptyUsers.ToTargetsAsync<UserDto, UserDtoAsyncMapper>();

        // Assert
        results.ShouldNotBeNull();
        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToTargetsAsync_ShouldHandleCancellation()
    {
        // Arrange
        var users = new List<User>
        {
            TestDataFactory.CreateUser("Test1", "User1"),
            TestDataFactory.CreateUser("Test2", "User2")
        };
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(5)); // Cancel quickly

        // Act & Assert
        var act = () => users.ToTargetsAsync<UserDto, UserDtoAsyncMapper>(cts.Token);
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ToTargetsParallelAsync_ShouldMapCollectionInParallel()
    {
        // Arrange
        var users = new List<User>
        {
            TestDataFactory.CreateUser("John", "Doe", "john@example.com"),
            TestDataFactory.CreateUser("Jane", "Smith", "jane@example.com"),
            TestDataFactory.CreateUser("Bob", "Johnson", "bob@example.com"),
            TestDataFactory.CreateUser("Alice", "Williams", "alice@example.com")
        };

        // Act
        var results = await users.ToTargetsParallelAsync<UserDto, UserDtoAsyncMapper>(
            maxDegreeOfParallelism: 2);

        // Assert
        results.ShouldNotBeNull();
        results.Count().ShouldBe(4);
        
        // Verify all users are mapped correctly (order might change due to parallel processing)
        results.ShouldContain(r => r.FirstName == "John" && r.FullName == "John Doe");
        results.ShouldContain(r => r.FirstName == "Jane" && r.FullName == "Jane Smith");
        results.ShouldContain(r => r.FirstName == "Bob" && r.FullName == "Bob Johnson");
        results.ShouldContain(r => r.FirstName == "Alice" && r.FullName == "Alice Williams");
    }

    [Fact]
    public async Task ToTargetsParallelAsync_ShouldUseDefaultParallelism()
    {
        // Arrange
        var users = new List<User>
        {
            TestDataFactory.CreateUser("User1", "Test1"),
            TestDataFactory.CreateUser("User2", "Test2")
        };

        // Act
        var results = await users.ToTargetsParallelAsync<UserDto, UserDtoAsyncMapper>();

        // Assert
        results.ShouldNotBeNull();
        results.Count().ShouldBe(2);
        results.All(r => !string.IsNullOrEmpty(r.FullName)).ShouldBeTrue();
    }

    [Fact]
    public async Task ToTargetsParallelAsync_ShouldHandleCancellation()
    {
        // Arrange
        var users = new List<User>
        {
            TestDataFactory.CreateUser("Test1", "User1"),
            TestDataFactory.CreateUser("Test2", "User2")
        };
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var act = () => users.ToTargetsParallelAsync<UserDto, UserDtoAsyncMapper>(
            cancellationToken: cts.Token);
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ToTargetHybridAsync_ShouldApplyBothSyncAndAsyncMapping()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com", new DateTime(1990, 1, 1));

        // Act
        var result = await user.ToTargetHybridAsync<UserDto, UserDtoHybridMapper>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.FirstName.ShouldBe("John");
        result.LastName.ShouldBe("Doe");
        result.Email.ShouldBe("john@example.com");
        
        // Sync mapping results with async modification
        result.FullName.ShouldBe("John Doe (Hybrid)"); // Modified by async part
        result.Age.ShouldBeGreaterThan(30);
    }

    [Fact]
    public async Task ToTargetHybridAsync_ShouldHandleCancellation()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var act = () => user.ToTargetHybridAsync<UserDto, UserDtoHybridMapper>(cts.Token);
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ToTargetHybridAsync_ShouldCalculateCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-30).AddDays(-100); // 30+ years old
        var user = TestDataFactory.CreateUser("Alice", "Johnson", dateOfBirth: birthDate);

        // Act
        var result = await user.ToTargetHybridAsync<UserDto, UserDtoHybridMapper>();

        // Assert
        result.Age.ShouldBe(30);
        result.FullName.ShouldBe("Alice Johnson (Hybrid)");
    }

    [Fact]
    public async Task ToTargetAsync_ShouldThrowWhenSourceIsNull()
    {
        // Arrange
        User nullUser = null!;

        // Act & Assert
        var act = () => nullUser.ToTargetAsync<UserDto, UserDtoAsyncMapper>();
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("source");
    }

    [Fact]
    public async Task ToTargetsAsync_ShouldThrowWhenSourceIsNull()
    {
        // Arrange
        System.Collections.IEnumerable nullUsers = null!;

        // Act & Assert
        var act = () => nullUsers.ToTargetsAsync<UserDto, UserDtoAsyncMapper>();
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("source");
    }

    [Fact]
    public async Task ToTargetsParallelAsync_ShouldThrowWhenSourceIsNull()
    {
        // Arrange
        System.Collections.IEnumerable nullUsers = null!;

        // Act & Assert
        var act = () => nullUsers.ToTargetsParallelAsync<UserDto, UserDtoAsyncMapper>();
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("source");
    }

    [Fact]
    public async Task ToTargetHybridAsync_ShouldThrowWhenSourceIsNull()
    {
        // Arrange
        User nullUser = null!;

        // Act & Assert
        var act = () => nullUser.ToTargetHybridAsync<UserDto, UserDtoHybridMapper>();
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("source");
    }

    [Fact]
    public async Task SimplifiedSyntax_ShouldProduceEquivalentResults_ToExplicitSyntax()
    {
        // Arrange
        var user = TestDataFactory.CreateUser("John", "Doe", "john@example.com");

        // Act - Compare both syntaxes
        var explicitResult = await user.ToTargetAsync<User, UserDto, UserDtoAsyncMapper>();
        var simplifiedResult = await user.ToTargetAsync<UserDto, UserDtoAsyncMapper>();

        // Assert
        explicitResult.FirstName.ShouldBe(simplifiedResult.FirstName);
        explicitResult.LastName.ShouldBe(simplifiedResult.LastName);
        explicitResult.Email.ShouldBe(simplifiedResult.Email);
        explicitResult.FullName.ShouldBe(simplifiedResult.FullName);
        explicitResult.Age.ShouldBe(simplifiedResult.Age);
        
        // Check that both have the expected computed fields
        explicitResult.FullName.ShouldBe("John Doe");
        simplifiedResult.FullName.ShouldBe("John Doe");
    }
}
