using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions.EFCore;

public class StreamingTests : IDisposable
{
    private readonly DbContext _context;

    public StreamingTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        SeedTestData();
    }

    [Fact]
    public async Task AsAsyncEnumerable_WithSelectTarget_ShouldStreamResults()
    {
        // Arrange & Act
        var userDtos = new List<UserDto>();

        await foreach (var dto in _context.Set<User>()
            .Where(u => u.IsActive)
            .SelectTarget<User, UserDto>()
            .AsAsyncEnumerable())
        {
            userDtos.Add(dto);
        }

        // Assert
        userDtos.Count().ShouldBe(2);
        userDtos.All(dto => dto.IsActive).ShouldBeTrue();
        userDtos.Select(dto => dto.FirstName).ShouldBe(["Alice", "Bob"]);
    }

    [Fact]
    public async Task AsAsyncEnumerable_WithNonGenericSelectTarget_ShouldStreamResults()
    {
        // Arrange & Act
        var userDtos = new List<UserDto>();

        await foreach (var dto in _context.Set<User>()
            .Where(u => u.FirstName == "Alice")
            .SelectTarget<UserDto>()
            .AsAsyncEnumerable())
        {
            userDtos.Add(dto);
        }

        // Assert
        userDtos.Count().ShouldBe(1);
        userDtos.First().FirstName.ShouldBe("Alice");
    }

    [Fact]
    public async Task AsAsyncEnumerable_WithComplexQuery_ShouldStreamResults()
    {
        // Arrange & Act
        var userDtos = new List<UserDto>();

        await foreach (var dto in _context.Set<User>()
            .Where(u => u.Email.Contains("@"))
            .OrderBy(u => u.FirstName)
            .SelectTarget<User, UserDto>()
            .AsAsyncEnumerable())
        {
            userDtos.Add(dto);
        }

        // Assert
        userDtos.Count().ShouldBe(3);
        userDtos[0].FirstName.ShouldBe("Alice");
        userDtos[1].FirstName.ShouldBe("Bob");
        userDtos[2].FirstName.ShouldBe("Charlie");
    }

    [Fact]
    public async Task AsAsyncEnumerable_WithNestedTargets_ShouldStreamResults()
    {
        // Arrange
        var address = new AddressEntity
        {
            Street = "123 Stream St",
            City = "Stream City",
            State = "ST",
            ZipCode = "11111",
            Country = "Streamland"
        };

        var company = new CompanyEntity
        {
            Id = 100,
            Name = "Stream Company",
            Industry = "Streaming",
            HeadquartersAddress = address
        };

        _context.Set<AddressEntity>().Add(address);
        _context.Set<CompanyEntity>().Add(company);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        var companyDtos = new List<CompanyTarget>();

        await foreach (var dto in _context.Set<CompanyEntity>()
            .Where(c => c.Id == 100)
            .SelectTarget<CompanyEntity, CompanyTarget>()
            .AsAsyncEnumerable())
        {
            companyDtos.Add(dto);
        }

        // Assert
        companyDtos.Count().ShouldBe(1);
        companyDtos.First().Name.ShouldBe("Stream Company");
        companyDtos.First().HeadquartersAddress.ShouldNotBeNull();
        companyDtos.First().HeadquartersAddress.City.ShouldBe("Stream City");
    }

    [Fact]
    public async Task AsAsyncEnumerable_WithTake_ShouldStreamLimitedResults()
    {
        // Arrange & Act
        var userDtos = new List<UserDto>();
        var count = 0;

        await foreach (var dto in _context.Set<User>()
            .OrderBy(u => u.Id)
            .Take(2)
            .SelectTarget<User, UserDto>()
            .AsAsyncEnumerable())
        {
            userDtos.Add(dto);
            count++;
        }

        // Assert
        count.ShouldBe(2);
        userDtos.Count().ShouldBe(2);
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
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
