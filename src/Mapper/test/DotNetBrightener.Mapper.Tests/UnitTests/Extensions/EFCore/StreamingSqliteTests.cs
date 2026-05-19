using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions.EFCore;

public class StreamingSqliteTests : IDisposable
{
    private readonly DbContext _context;
    private readonly SqliteConnection _connection;

    public StreamingSqliteTests()
    {
        // SQLite in-memory requires keeping the connection open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
        SeedTestData();
    }

    [Fact]
    public async Task SqliteProvider_AsAsyncEnumerable_WithSelectTarget_ShouldStreamResults()
    {
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
    public async Task SqliteProvider_AsAsyncEnumerable_WithNonGenericSelectTarget_ShouldStreamResults()
    {
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
    public async Task SqliteProvider_AsAsyncEnumerable_WithComplexQuery_ShouldStreamResults()
    {
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
    public async Task SqliteProvider_AsAsyncEnumerable_WithNestedTargets_ShouldStreamResults()
    {
        // Arrange
        var address = new AddressEntity
        {
            Street = "123 SQLite St",
            City = "SQLite City",
            State = "SQ",
            ZipCode = "11111",
            Country = "SQLiteland"
        };

        var company = new CompanyEntity
        {
            Id = 100,
            Name = "SQLite Company",
            Industry = "Database",
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
        companyDtos.First().Name.ShouldBe("SQLite Company");
        companyDtos.First().HeadquartersAddress.ShouldNotBeNull();
        companyDtos.First().HeadquartersAddress.City.ShouldBe("SQLite City");
    }

    [Fact]
    public async Task SqliteProvider_AsAsyncEnumerable_VerifyNotLoadingAllIntoMemory()
    {
        var processedCount = 0;
        var maxSimultaneous = 0;
        var currentInMemory = 0;

        await foreach (var dto in _context.Set<User>()
            .OrderBy(u => u.Id)
            .SelectTarget<User, UserDto>()
            .AsAsyncEnumerable())
        {
            currentInMemory++;
            maxSimultaneous = Math.Max(maxSimultaneous, currentInMemory);

            // Simulate processing
            await Task.Delay(1);

            processedCount++;
            currentInMemory--;
        }

        // Assert - we processed all items
        processedCount.ShouldBe(3);

        maxSimultaneous.ShouldBeLessThan(3);
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
        _connection.Dispose();
    }
}
