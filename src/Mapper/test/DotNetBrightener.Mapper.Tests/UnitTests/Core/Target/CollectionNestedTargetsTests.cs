using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class CollectionNestedTargetsTests
{
    [Fact]
    public void TargetConstructor_ShouldMapListCollection_WhenUsingNestedTargets()
    {
        // Arrange
        var order = new OrderEntity
        {
            Id = 1,
            OrderNumber = "ORD-2025-001",
            OrderDate = new DateTime(2025, 1, 15),
            Items =
            [
                new()
                {
                    Id          = 1,
                    ProductName = "Laptop",
                    Price       = 1200.00m,
                    Quantity    = 1
                },
                new()
                {
                    Id          = 2,
                    ProductName = "Mouse",
                    Price       = 25.00m,
                    Quantity    = 2
                },
                new()
                {
                    Id          = 3,
                    ProductName = "Keyboard",
                    Price       = 75.00m,
                    Quantity    = 1
                }
            ],
            ShippingAddress = new AddressEntity
            {
                Street = "123 Main St",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101",
                Country = "USA"
            }
        };

        // Act
        var dto = new OrderTarget(order);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.OrderNumber.ShouldBe("ORD-2025-001");
        dto.OrderDate.ShouldBe(new DateTime(2025, 1, 15));

        // Verify collection mapping
        dto.Items.ShouldNotBeNull();
        dto.Items.Count().ShouldBe(3);
        dto.Items.ShouldAllBe(x => x is OrderItemTarget);

        // Verify first item
        dto.Items[0].Id.ShouldBe(1);
        dto.Items[0].ProductName.ShouldBe("Laptop");
        dto.Items[0].Price.ShouldBe(1200.00m);
        dto.Items[0].Quantity.ShouldBe(1);

        // Verify second item
        dto.Items[1].Id.ShouldBe(2);
        dto.Items[1].ProductName.ShouldBe("Mouse");
        dto.Items[1].Price.ShouldBe(25.00m);
        dto.Items[1].Quantity.ShouldBe(2);

        // Verify nested address
        dto.ShippingAddress.ShouldNotBeNull();
        dto.ShippingAddress.Street.ShouldBe("123 Main St");
        dto.ShippingAddress.City.ShouldBe("Seattle");
    }

    [Fact]
    public void TargetConstructor_ShouldMapArrayCollection_WhenUsingNestedTargets()
    {
        // Arrange
        var team = new TeamEntity
        {
            Id = 10,
            Name = "Development Team",
            Members =
            [
                new StaffMember
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Email = "alice@example.com",
                    PasswordHash = "hash1",
                    Salary = 90000m,
                    HireDate = new DateTime(2020, 1, 1),
                    Company = new CompanyEntity { Id = 100, Name = "Tech Corp", Industry = "Technology", HeadquartersAddress = new AddressEntity() },
                    HomeAddress = new AddressEntity { City = "Seattle" }
                },
                new StaffMember
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Email = "bob@example.com",
                    PasswordHash = "hash2",
                    Salary = 95000m,
                    HireDate = new DateTime(2019, 5, 15),
                    Company = new CompanyEntity { Id = 100, Name = "Tech Corp", Industry = "Technology", HeadquartersAddress = new AddressEntity() },
                    HomeAddress = new AddressEntity { City = "Portland" }
                }
            ]
        };

        // Act
        var dto = new TeamTarget(team);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(10);
        dto.Name.ShouldBe("Development Team");

        // Verify array mapping
        dto.Members.ShouldNotBeNull();
        dto.Members.Count().ShouldBe(2);
        dto.Members.ShouldAllBe(x => x is StaffMemberTarget);

        // Verify first member (PasswordHash and Salary should be excluded)
        dto.Members[0].Id.ShouldBe(1);
        dto.Members[0].FirstName.ShouldBe("Alice");
        dto.Members[0].LastName.ShouldBe("Johnson");
        dto.Members[0].Email.ShouldBe("alice@example.com");
        dto.Members[0].HireDate.ShouldBe(new DateTime(2020, 1, 1));

        var dtoType = dto.Members[0].GetType();
        dtoType.GetProperty("PasswordHash").ShouldBeNull("PasswordHash should be excluded");
        dtoType.GetProperty("Salary").ShouldBeNull("Salary should be excluded");

        // Verify second member
        dto.Members[1].FirstName.ShouldBe("Bob");
        dto.Members[1].LastName.ShouldBe("Smith");
    }

    [Fact]
    public void TargetConstructor_ShouldMapICollectionType_WhenUsingNestedTargets()
    {
        // Arrange
        var project = new ProjectEntity
        {
            Id = 500,
            Name = "Project Phoenix",
            Teams = new List<TeamEntity>
            {
                new()
                {
                    Id = 1,
                    Name = "Backend Team",
                    Members =
                    [
                        new StaffMember
                        {
                            Id = 10,
                            FirstName = "Charlie",
                            LastName = "Brown",
                            Email = "charlie@example.com",
                            PasswordHash = "hash",
                            Salary = 100000m,
                            Company = new CompanyEntity { Id = 1, Name = "Corp", Industry = "Tech", HeadquartersAddress = new AddressEntity() },
                            HomeAddress = new AddressEntity()
                        }
                    ]
                },
                new()
                {
                    Id = 2,
                    Name = "Frontend Team",
                    Members = []
                }
            }
        };

        // Act
        var dto = new ProjectTarget(project);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(500);
        dto.Name.ShouldBe("Project Phoenix");

        // Verify ICollection mapping
        dto.Teams.ShouldNotBeNull();
        dto.Teams.Count().ShouldBe(2);
        dto.Teams.ShouldAllBe(x => x is TeamTarget);

        // Verify first team
        var firstTeam = dto.Teams.ElementAt(0);
        firstTeam.Id.ShouldBe(1);
        firstTeam.Name.ShouldBe("Backend Team");
        firstTeam.Members.Count().ShouldBe(1);
        firstTeam.Members[0].FirstName.ShouldBe("Charlie");

        // Verify second team
        var secondTeam = dto.Teams.ElementAt(1);
        secondTeam.Id.ShouldBe(2);
        secondTeam.Name.ShouldBe("Frontend Team");
        secondTeam.Members.ShouldBeEmpty();
    }

    [Fact]
    public void TargetConstructor_ShouldHandleEmptyCollections_WhenUsingNestedTargets()
    {
        // Arrange
        var order = new OrderEntity
        {
            Id              = 1,
            OrderNumber     = "ORD-EMPTY",
            OrderDate       = DateTime.Now,
            Items           = [],
            ShippingAddress = new AddressEntity()
        };

        // Act
        var dto = new OrderTarget(order);

        // Assert
        dto.ShouldNotBeNull();
        dto.Items.ShouldNotBeNull();
        dto.Items.ShouldBeEmpty();
    }

    [Fact]
    public void BackTo_ShouldMapListCollectionBackToSource()
    {
        // Arrange
        var order = new OrderEntity
        {
            Id = 1,
            OrderNumber = "ORD-2025-001",
            OrderDate = new DateTime(2025, 1, 15),
            Items =
            [
                new()
                {
                    Id          = 1,
                    ProductName = "Laptop",
                    Price       = 1200.00m,
                    Quantity    = 1
                },
                new()
                {
                    Id          = 2,
                    ProductName = "Mouse",
                    Price       = 25.00m,
                    Quantity    = 2
                }
            ],
            ShippingAddress = new AddressEntity
            {
                Street = "123 Main St",
                City = "Seattle",
                State = "WA",
                ZipCode = "98101",
                Country = "USA"
            }
        };

        var dto = new OrderTarget(order);

        // Act
        var mappedOrder = dto.ToSource();

        // Assert
        mappedOrder.ShouldNotBeNull();
        mappedOrder.Id.ShouldBe(1);
        mappedOrder.OrderNumber.ShouldBe("ORD-2025-001");
        mappedOrder.Items.Count().ShouldBe(2);
        mappedOrder.Items.ShouldAllBe(x => x is OrderItemEntity);

        mappedOrder.Items[0].Id.ShouldBe(1);
        mappedOrder.Items[0].ProductName.ShouldBe("Laptop");
        mappedOrder.Items[0].Price.ShouldBe(1200.00m);

        mappedOrder.ShippingAddress.Street.ShouldBe("123 Main St");
    }

    [Fact]
    public void ToSource_ShouldMapArrayCollectionBackToSource()
    {
        // Arrange
        var team = new TeamEntity
        {
            Id = 10,
            Name = "Development Team",
            Members =
            [
                new StaffMember
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Email = "alice@example.com",
                    PasswordHash = "hash1",
                    Salary = 90000m,
                    HireDate = new DateTime(2020, 1, 1),
                    Company = new CompanyEntity { Id = 100, Name = "Tech Corp", Industry = "Technology", HeadquartersAddress = new AddressEntity() },
                    HomeAddress = new AddressEntity { City = "Seattle" }
                }
            ]
        };

        var dto = new TeamTarget(team);

        // Act
        var mappedTeam = dto.ToSource();

        // Assert
        mappedTeam.ShouldNotBeNull();
        mappedTeam.Id.ShouldBe(10);
        mappedTeam.Name.ShouldBe("Development Team");
        mappedTeam.Members.Count().ShouldBe(1);
        mappedTeam.Members.ShouldBeOfType<StaffMember[]>();

        mappedTeam.Members[0].FirstName.ShouldBe("Alice");
        mappedTeam.Members[0].LastName.ShouldBe("Johnson");
    }

    [Fact]
    public void ToSource_ShouldMapICollectionBackToSource()
    {
        // Arrange
        var project = new ProjectEntity
        {
            Id = 500,
            Name = "Project Phoenix",
            Teams = new List<TeamEntity>
            {
                new() { Id = 1, Name = "Backend Team", Members  = [] },
                new() { Id = 2, Name = "Frontend Team", Members = [] }
            }
        };

        var dto = new ProjectTarget(project);

        // Act
        var mappedProject = dto.ToSource();

        // Assert
        mappedProject.ShouldNotBeNull();
        mappedProject.Id.ShouldBe(500);
        mappedProject.Name.ShouldBe("Project Phoenix");
        mappedProject.Teams.Count().ShouldBe(2);
        mappedProject.Teams.ShouldAllBe(x => x is TeamEntity);

        mappedProject.Teams.ElementAt(0).Name.ShouldBe("Backend Team");
        mappedProject.Teams.ElementAt(1).Name.ShouldBe("Frontend Team");
    }

    [Fact]
    public void Collection_ShouldPreserveTypeFromSource()
    {
        // Arrange
        var order = new OrderEntity
        {
            Id              = 1,
            OrderNumber     = "ORD-001",
            OrderDate       = DateTime.Now,
            Items           =
            [
                new()
                {
                    Id          = 1,
                    ProductName = "Item1",
                    Price       = 10,
                    Quantity    = 1
                }
            ],
            ShippingAddress = new AddressEntity()
        };

        // Act
        var dto = new OrderTarget(order);

        // Assert - Items should be List<OrderItemTarget>
        dto.Items.ShouldBeAssignableTo<List<OrderItemTarget>>();
    }

    [Fact]
    public void TargetConstructor_ShouldMapIReadOnlyListCollection_WhenUsingNestedTargets()
    {
        // Arrange
        var library = new LibraryEntity
        {
            Id = 1,
            Name = "City Library",
            Books = new List<LibraryBookEntity>
            {
                new() { Id = 1, Title = "1984", Author = "George Orwell", ISBN = "978-0451524935" },
                new() { Id = 2, Title = "Brave New World", Author = "Aldous Huxley", ISBN = "978-0060850524" },
                new() { Id = 3, Title = "Fahrenheit 451", Author = "Ray Bradbury", ISBN = "978-1451673319" }
            },
            Staff = new List<StaffMember>
            {
                new()
                {
                    Id = 100,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane@library.com",
                    PasswordHash = "hash123",
                    Salary = 50000m,
                    HireDate = new DateTime(2020, 6, 1),
                    Company = new CompanyEntity { Id = 1, Name = "Library Corp", Industry = "Education", HeadquartersAddress = new AddressEntity() },
                    HomeAddress = new AddressEntity { City = "New York" }
                }
            }
        };

        // Act
        var dto = new LibraryTarget(library);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("City Library");

        // Verify IReadOnlyList<T> mapping
        dto.Books.ShouldNotBeNull();
        dto.Books.Count().ShouldBe(3);
        dto.Books.ShouldAllBe(x => x is LibraryBookTarget);
        dto.Books.ShouldBeAssignableTo<IReadOnlyList<LibraryBookTarget>>();

        // Verify first book
        dto.Books[0].Id.ShouldBe(1);
        dto.Books[0].Title.ShouldBe("1984");
        dto.Books[0].Author.ShouldBe("George Orwell");
        dto.Books[0].ISBN.ShouldBe("978-0451524935");

        // Verify IReadOnlyCollection<T> mapping
        dto.Staff.ShouldNotBeNull();
        dto.Staff.Count().ShouldBe(1);
        dto.Staff.ShouldAllBe(x => x is StaffMemberTarget);
        dto.Staff.ShouldBeAssignableTo<IReadOnlyCollection<StaffMemberTarget>>();

        var staffMember = dto.Staff.First();
        staffMember.Id.ShouldBe(100);
        staffMember.FirstName.ShouldBe("Jane");
        staffMember.LastName.ShouldBe("Doe");
        staffMember.Email.ShouldBe("jane@library.com");

        // Verify excluded properties
        var staffType = staffMember.GetType();
        staffType.GetProperty("PasswordHash").ShouldBeNull("PasswordHash should be excluded");
        staffType.GetProperty("Salary").ShouldBeNull("Salary should be excluded");
    }

    [Fact]
    public void ToSource_ShouldMapIReadOnlyListCollectionBackToSource()
    {
        // Arrange
        var library = new LibraryEntity
        {
            Id = 1,
            Name = "City Library",
            Books = new List<LibraryBookEntity>
            {
                new() { Id = 1, Title = "1984", Author = "George Orwell", ISBN = "978-0451524935" },
                new() { Id = 2, Title = "Brave New World", Author = "Aldous Huxley", ISBN = "978-0060850524" }
            },
            Staff = new List<StaffMember>()
        };

        var dto = new LibraryTarget(library);

        // Act
        var mappedLibrary = dto.ToSource();

        // Assert
        mappedLibrary.ShouldNotBeNull();
        mappedLibrary.Id.ShouldBe(1);
        mappedLibrary.Name.ShouldBe("City Library");

        // Verify IReadOnlyList mapping back
        mappedLibrary.Books.ShouldNotBeNull();
        mappedLibrary.Books.Count().ShouldBe(2);
        mappedLibrary.Books.ShouldAllBe(x => x is LibraryBookEntity);
        mappedLibrary.Books.ShouldBeAssignableTo<IReadOnlyList<LibraryBookEntity>>();

        mappedLibrary.Books[0].Id.ShouldBe(1);
        mappedLibrary.Books[0].Title.ShouldBe("1984");
        mappedLibrary.Books[0].Author.ShouldBe("George Orwell");

        // Verify IReadOnlyCollection mapping back
        mappedLibrary.Staff.ShouldNotBeNull();
        mappedLibrary.Staff.ShouldBeEmpty();
        mappedLibrary.Staff.ShouldBeAssignableTo<IReadOnlyCollection<StaffMember>>();
    }

    [Fact]
    public void IReadOnlyList_ShouldWorkWithNestedTargets_ReproducesIssue218()
    {
        // This test reproduces the exact scenario from GitHub issue #218
        // Arrange
        var bob = new Bob
        {
            ReadOnlyRelationships = new List<BobChild>
            {
                new() { Name = "Alice" },
                new() { Name = "Charlie" }
            },
            Relationships =
            [
                new()
                {
                    Name = "Dave"
                },
                new()
                {
                    Name = "Eve"
                }
            ]
        };

        // Act
        var bobModel = new BobModel(bob);

        // Assert
        bobModel.ShouldNotBeNull();

        // Verify IReadOnlyList<BobChild> was correctly mapped to IReadOnlyList<BobChildModel>
        bobModel.ReadOnlyRelationships.ShouldNotBeNull();
        bobModel.ReadOnlyRelationships.Count().ShouldBe(2);
        bobModel.ReadOnlyRelationships.ShouldAllBe(x => x is BobChildModel);
        bobModel.ReadOnlyRelationships.ShouldBeAssignableTo<IReadOnlyList<BobChildModel>>();
        bobModel.ReadOnlyRelationships[0].Name.ShouldBe("Alice");
        bobModel.ReadOnlyRelationships[1].Name.ShouldBe("Charlie");

        // Verify List<BobChild> was correctly mapped to List<BobChildModel>
        bobModel.Relationships.ShouldNotBeNull();
        bobModel.Relationships.Count().ShouldBe(2);
        bobModel.Relationships.ShouldAllBe(x => x is BobChildModel);
        bobModel.Relationships.ShouldBeAssignableTo<List<BobChildModel>>();
        bobModel.Relationships[0].Name.ShouldBe("Dave");
        bobModel.Relationships[1].Name.ShouldBe("Eve");
    }
}
