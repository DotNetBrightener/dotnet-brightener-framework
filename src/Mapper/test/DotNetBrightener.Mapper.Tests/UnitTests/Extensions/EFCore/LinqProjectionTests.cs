using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions.EFCore;

public class LinqProjectionTests : IDisposable
{
    private readonly DbContext _context;

    public LinqProjectionTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        SeedTestData();
    }

    [Fact]
    public void Select_ShouldProjectToTarget_InLinqQueries()
    {
        // Arrange & Act
        var userDtos = _context.Set<User>()
            .Select(u => u.ToTarget<User, UserDto>())
            .ToList();

        // Assert
        userDtos.ShouldNotBeEmpty();
        userDtos.Count().ShouldBe(3);
        userDtos.All(dto => !string.IsNullOrEmpty(dto.FirstName)).ShouldBeTrue();
        userDtos.All(dto => !string.IsNullOrEmpty(dto.Email)).ShouldBeTrue();
        
        var dtoType = userDtos.First().GetType();
        dtoType.GetProperty("Password").ShouldBeNull();
        dtoType.GetProperty("CreatedAt").ShouldBeNull();
    }

    [Fact]
    public void Where_ThenSelect_ShouldFilterAndProjectToTarget()
    {
        // Arrange & Act
        var activeDtos = _context.Set<User>()
            .Where(u => u.IsActive)
            .Select(u => u.ToTarget<User, UserDto>())
            .ToList();

        // Assert
        activeDtos.Count().ShouldBe(2);
        activeDtos.All(dto => dto.IsActive).ShouldBeTrue();
        activeDtos.Select(dto => dto.FirstName).ShouldBe(["Alice", "Bob"]);
    }

    [Fact]
    public void OrderBy_ThenSelect_ShouldOrderAndProjectToTarget()
    {
        // Arrange & Act
        var orderedDtos = _context.Set<User>()
            .OrderBy(u => u.FirstName)
            .Select(u => u.ToTarget<User, UserDto>())
            .ToList();

        // Assert
        orderedDtos.Count().ShouldBe(3);
        orderedDtos[0].FirstName.ShouldBe("Alice");
        orderedDtos[1].FirstName.ShouldBe("Bob");
        orderedDtos[2].FirstName.ShouldBe("Charlie");
    }

    [Fact]
    public void Take_ThenSelect_ShouldLimitAndProjectToTarget()
    {
        // Arrange & Act
        var limitedDtos = _context.Set<User>()
            .OrderBy(u => u.Id)
            .Take(2)
            .Select(u => u.ToTarget<User, UserDto>())
            .ToList();

        // Assert
        limitedDtos.Count().ShouldBe(2);
    }

    [Fact]
    public void GroupBy_ThenSelect_ShouldGroupAndProjectToTarget()
    {
        // Arrange & Act
        var groupedResults = _context.Set<User>()
            .GroupBy(u => u.IsActive)
            .Select(g => new
            {
                IsActive = g.Key,
                Users = g.Select(u => u.ToTarget<User, UserDto>()).ToList(),
                Count = g.Count()
            })
            .ToList();

        // Assert
        groupedResults.Count().ShouldBe(2);
        
        var activeGroup = groupedResults.First(g => g.IsActive);
        var inactiveGroup = groupedResults.First(g => !g.IsActive);
        
        activeGroup.Count.ShouldBe(2);
        activeGroup.Users.Count().ShouldBe(2);
        activeGroup.Users.All(u => u.IsActive).ShouldBeTrue();
        
        inactiveGroup.Count.ShouldBe(1);
        inactiveGroup.Users.Count().ShouldBe(1);
        inactiveGroup.Users.All(u => !u.IsActive).ShouldBeTrue();
    }

    [Fact]
    public void Join_ThenSelect_ShouldJoinAndProjectToTarget()
    {
        // Arrange & Act
        var joinResults = _context.Set<User>()
            .Join(_context.Set<Product>(),
                user => user.Id,
                product => product.CategoryId,
                (user, product) => new
                {
                    User = user.ToTarget<User, UserDto>(),
                    Product = product.ToTarget<Product, ProductDto>()
                })
            .ToList();

        // Assert
        joinResults.ShouldNotBeEmpty();
        joinResults.All(r => r.User != null).ShouldBeTrue();
        joinResults.All(r => r.Product != null).ShouldBeTrue();
        joinResults.All(r => r.User.GetType().GetProperty("Password") == null).ShouldBeTrue();
        joinResults.All(r => r.Product.GetType().GetProperty("InternalNotes") == null).ShouldBeTrue();
    }

    [Fact]
    public void ComplexQuery_WithMultipleOperations_ShouldProjectToTargetCorrectly()
    {
        // Arrange & Act
        var complexResults = _context.Set<User>()
            .Where(u => u.Email.Contains("@"))
            .OrderByDescending(u => u.DateOfBirth)
            .Skip(1)
            .Take(1)
            .Select(u => u.ToTarget<User, UserDto>())
            .ToList();

        // Assert
        complexResults.Count().ShouldBe(1);
        complexResults.First().Email.ShouldContain("@");
    }

    [Fact]
    public async Task SelectAsync_ShouldProjectToTarget_InAsyncQueries()
    {
        // Arrange & Act
        var userDtos = await _context.Set<User>()
            .Select(u => u.ToTarget<User, UserDto>())
            .ToListAsync();

        // Assert
        userDtos.ShouldNotBeEmpty();
        userDtos.Count().ShouldBe(3);
        userDtos.All(dto => !string.IsNullOrEmpty(dto.FirstName)).ShouldBeTrue();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldProjectToTarget()
    {
        // Arrange & Act
        var firstDto = await _context.Set<User>()
            .Where(u => u.FirstName == "Alice")
            .Select(u => u.ToTarget<User, UserDto>())
            .FirstOrDefaultAsync();

        // Assert
        firstDto.ShouldNotBeNull();
        firstDto!.FirstName.ShouldBe("Alice");
        firstDto.GetType().GetProperty("Password").ShouldBeNull();
    }

    private void SeedTestData()
    {
        var baseId = Random.Shared.Next(1000, 9999);
        var users = new List<User>
        {
            TestDataFactory.CreateUser("Alice", "Johnson", "alice.johnson@example.com", new DateTime(1985, 3, 22), true),
            TestDataFactory.CreateUser("Bob", "Smith", "bob.smith@example.com", new DateTime(1992, 8, 10), true),
            TestDataFactory.CreateUser("Charlie", "Brown", "charlie.brown@example.com", new DateTime(1988, 12, 5), false)
        };
        
        for (int i = 0; i < users.Count; i++)
        {
            users[i].Id = baseId + i;
        }
        
        _context.Set<User>().AddRange(users);

        var products = new List<Product>
        {
            TestDataFactory.CreateProduct("Product 1"),
            TestDataFactory.CreateProduct("Product 2"),
            TestDataFactory.CreateProduct("Product 3")
        };
        
        // Ensure unique product IDs and link to users
        for (int i = 0; i < products.Count && i < users.Count; i++)
        {
            products[i].Id = baseId + 100 + i;
            products[i].CategoryId = users[i].Id;
        }
        
        _context.Set<Product>().AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public void Projection_WithNestedTargets_ShouldLoadNavigationPropertiesWithoutInclude()
    {
        // Arrange
        var address = new AddressEntity
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            ZipCode = "12345",
            Country = "Testland"
        };

        var company = new CompanyEntity
        {
            Id = 1,
            Name = "Test Company",
            Industry = "Technology",
            HeadquartersAddress = address
        };

        _context.Set<AddressEntity>().Add(address);
        _context.Set<CompanyEntity>().Add(company);
        _context.SaveChanges();

        // Clear the context to ensure we're not using cached entities
        _context.ChangeTracker.Clear();

        // Act - Use projection WITHOUT .Include()
        var companyDto = _context.Set<CompanyEntity>()
            .Where(c => c.Id == 1)
            .Select(CompanyTarget.Projection)
            .FirstOrDefault();

        // Assert
        companyDto.ShouldNotBeNull();
        companyDto!.Id.ShouldBe(1);
        companyDto.Name.ShouldBe("Test Company");
        companyDto.Industry.ShouldBe("Technology");

        // The nested target should be loaded and mapped
        companyDto.HeadquartersAddress.ShouldNotBeNull();
        companyDto.HeadquartersAddress.Street.ShouldBe("123 Main St");
        companyDto.HeadquartersAddress.City.ShouldBe("Test City");
        companyDto.HeadquartersAddress.State.ShouldBe("TS");
        companyDto.HeadquartersAddress.ZipCode.ShouldBe("12345");
        companyDto.HeadquartersAddress.Country.ShouldBe("Testland");
    }

    [Fact]
    public void SelectTarget_WithNestedTargets_ShouldLoadNavigationPropertiesWithoutInclude()
    {
        // Arrange
        var address = new AddressEntity
        {
            Street = "789 SelectTarget Ave",
            City = "SelectTarget City",
            State = "SF",
            ZipCode = "99999",
            Country = "Selectland"
        };

        var company = new CompanyEntity
        {
            Id = 2,
            Name = "SelectTarget Company",
            Industry = "Software",
            HeadquartersAddress = address
        };

        _context.Set<AddressEntity>().Add(address);
        _context.Set<CompanyEntity>().Add(company);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        var companyDto = _context.Set<CompanyEntity>()
            .Where(c => c.Id == 2)
            .SelectTarget<CompanyEntity, CompanyTarget>()
            .FirstOrDefault();

        // Assert
        companyDto.ShouldNotBeNull();
        companyDto!.Name.ShouldBe("SelectTarget Company");

        // The nested target should be loaded via SelectTarget
        companyDto.HeadquartersAddress.ShouldNotBeNull();
        companyDto.HeadquartersAddress.Street.ShouldBe("789 SelectTarget Ave");
        companyDto.HeadquartersAddress.City.ShouldBe("SelectTarget City");
    }

    [Fact]
    public void SelectTarget_NonGeneric_WithNestedTargets_ShouldLoadNavigationPropertiesWithoutInclude()
    {
        // Arrange
        var address = new AddressEntity
        {
            Street = "456 NonGeneric St",
            City = "NonGeneric City",
            State = "NG",
            ZipCode = "88888",
            Country = "NGland"
        };

        var company = new CompanyEntity
        {
            Id = 3,
            Name = "NonGeneric Company",
            Industry = "Finance",
            HeadquartersAddress = address
        };

        _context.Set<AddressEntity>().Add(address);
        _context.Set<CompanyEntity>().Add(company);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        IQueryable nonTypedQuery = _context.Set<CompanyEntity>().Where(c => c.Id == 3);
        var companyDto = nonTypedQuery
            .SelectTarget<CompanyTarget>()
            .FirstOrDefault();

        // Assert
        companyDto.ShouldNotBeNull();
        companyDto!.Name.ShouldBe("NonGeneric Company");

        // The nested target should be loaded
        companyDto.HeadquartersAddress.ShouldNotBeNull();
        companyDto.HeadquartersAddress.Street.ShouldBe("456 NonGeneric St");
        companyDto.HeadquartersAddress.City.ShouldBe("NonGeneric City");
    }

    [Fact]
    public void Projection_WithCollectionNestedTargets_ShouldLoadCollectionWithoutInclude()
    {
        // Arrange
        var order = new OrderEntity
        {
            Id = 1,
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow,
            Items =
            [
                new OrderItemEntity
                {
                    Id          = 1,
                    ProductName = "Item 1",
                    Price       = 10.00m,
                    Quantity    = 2
                },
                new OrderItemEntity
                {
                    Id          = 2,
                    ProductName = "Item 2",
                    Price       = 20.00m,
                    Quantity    = 1
                }
            ],
            ShippingAddress = new AddressEntity
            {
                Street = "456 Shipping Rd",
                City = "Ship City",
                State = "SC",
                ZipCode = "67890",
                Country = "Shipland"
            }
        };

        _context.Set<OrderEntity>().Add(order);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        var orderDto = _context.Set<OrderEntity>()
            .Where(o => o.Id == 1)
            .Select(OrderTarget.Projection)
            .FirstOrDefault();

        // Assert
        orderDto.ShouldNotBeNull();
        orderDto!.Id.ShouldBe(1);
        orderDto.OrderNumber.ShouldBe("ORD-001");

        // Collection nested targets should be loaded
        orderDto.Items.ShouldNotBeNull();
        orderDto.Items.Count().ShouldBe(2);
        orderDto.Items.ShouldContain(i => i.ProductName == "Item 1");
        orderDto.Items.ShouldContain(i => i.ProductName == "Item 2");

        // Single nested target should also be loaded
        orderDto.ShippingAddress.ShouldNotBeNull();
        orderDto.ShippingAddress.City.ShouldBe("Ship City");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<Product>().HasKey(p => p.Id);
        modelBuilder.Entity<Employee>().HasBaseType<User>();
        modelBuilder.Entity<Manager>().HasBaseType<Employee>();

        // Add entities for nested target tests
        modelBuilder.Entity<AddressEntity>().HasKey(a => new { a.Street, a.City, a.State });
        modelBuilder.Entity<CompanyEntity>().HasKey(c => c.Id);
        modelBuilder.Entity<OrderEntity>().HasKey(o => o.Id);
        modelBuilder.Entity<OrderItemEntity>().HasKey(i => i.Id);
    }
}
