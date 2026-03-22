using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

/// <summary>
///     Tests for issue #188: Nested [MappingTarget] classes with duplicate names support
/// </summary>
public class NestedClassDuplicateNamesTests
{
    [Fact]
    public void NestedClasses_WithSameName_InDifferentParents_ShouldGenerateSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "secret",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1),
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act - Create both nested DTOs
        var listItem = new UserListResponse.UserItem(user);
        var detailItem = new UserDetailResponse.UserItem(user);

        // Assert - List item should only have Id and FirstName
        listItem.ShouldNotBeNull();
        listItem.Id.ShouldBe(1);
        listItem.FirstName.ShouldBe("John");

        var listItemType = listItem.GetType();
        listItemType.GetProperty("LastName").ShouldBeNull("LastName should not be included in UserListResponse.UserItem");
        listItemType.GetProperty("Email").ShouldBeNull("Email should not be included in UserListResponse.UserItem");

        // Assert - Detail item should have Id, FirstName, LastName, and Email
        detailItem.ShouldNotBeNull();
        detailItem.Id.ShouldBe(1);
        detailItem.FirstName.ShouldBe("John");
        detailItem.LastName.ShouldBe("Doe");
        detailItem.Email.ShouldBe("john@example.com");

        var detailItemType = detailItem.GetType();
        detailItemType.GetProperty("Password").ShouldBeNull("Password should not be included in UserDetailResponse.UserItem");
    }

    [Fact]
    public void NestedClasses_WithSameName_ShouldHaveDifferentFullNames()
    {
        // Arrange & Act
        var listItemType = typeof(UserListResponse.UserItem);
        var detailItemType = typeof(UserDetailResponse.UserItem);

        // Assert
        listItemType.ShouldNotBe(detailItemType, "Types should be different even though they have the same simple name");
        listItemType.Name.ShouldBe("UserItem");
        detailItemType.Name.ShouldBe("UserItem");
        listItemType.FullName.ShouldContain("UserListResponse");
        detailItemType.FullName.ShouldContain("UserDetailResponse");
    }

    [Fact]
    public void NestedClasses_Projection_ShouldWorkCorrectly()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" },
            new User { Id = 2, FirstName = "Bob", LastName = "Jones", Email = "bob@test.com" }
        };

        // Act
        var listItems = users.Select(UserListResponse.UserItem.Projection.Compile()).ToList();
        var detailItems = users.Select(UserDetailResponse.UserItem.Projection.Compile()).ToList();

        // Assert
        listItems.Count().ShouldBe(2);
        listItems[0].FirstName.ShouldBe("Alice");
        listItems[1].FirstName.ShouldBe("Bob");

        detailItems.Count().ShouldBe(2);
        detailItems[0].FirstName.ShouldBe("Alice");
        detailItems[0].Email.ShouldBe("alice@test.com");
        detailItems[1].FirstName.ShouldBe("Bob");
        detailItems[1].Email.ShouldBe("bob@test.com");
    }

    [Fact]
    public void DeeplyNestedClasses_WithSameName_ShouldGenerateSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Test deeply nested classes
        var outerInner = new OuterClass1.InnerClass.Item(user);
        var anotherOuterInner = new OuterClass2.InnerClass.Item(user);

        // Assert
        outerInner.ShouldNotBeNull();
        outerInner.Id.ShouldBe(1);

        anotherOuterInner.ShouldNotBeNull();
        anotherOuterInner.Id.ShouldBe(1);

        typeof(OuterClass1.InnerClass.Item).ShouldNotBe(typeof(OuterClass2.InnerClass.Item));
    }
}

// Test models for nested classes with duplicate names
public partial class UserListResponse
{
    [MappingTarget<User>( Include = ["Id", "FirstName"])]
    public partial class UserItem;
}

public partial class UserDetailResponse
{
    [MappingTarget<User>( Include = ["Id", "FirstName", "LastName", "Email"])]
    public partial class UserItem;
}

// Test models for deeply nested classes
public partial class OuterClass1
{
    public partial class InnerClass
    {
        [MappingTarget<User>( Include = ["Id", "FirstName"])]
        public partial class Item;
    }
}

public partial class OuterClass2
{
    public partial class InnerClass
    {
        [MappingTarget<User>( Include = ["Id", "FirstName", "Email"])]
        public partial class Item;
    }
}
