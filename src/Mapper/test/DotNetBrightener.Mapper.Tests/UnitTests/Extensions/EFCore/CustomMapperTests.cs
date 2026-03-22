using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Mapping.Configurations;
using DotNetBrightener.Mapper.Mapping.EFCore;
using DotNetBrightener.Mapper.Tests.TestModels;
using DotNetBrightener.Mapper.Tests.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Extensions.EFCore;

public class CustomMapperTests : IDisposable
{
    private readonly DbContext _context;

    public CustomMapperTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        SeedTestData();
    }

    [Fact]
    public async Task ToTargetsAsync_WithInstanceMapper_ShouldApplyCustomMapping()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();

        // Act
        var users = await MapperAsyncExtensions.ToTargetsAsync(_context.Set<User>()
            .Where(u => u.IsActive)
, mapper);

        // Assert
        users.ShouldNotBeNull();
        users.Count().ShouldBe(2);
        users.All(u => u.FullName.Contains(" ")).ShouldBeTrue();
        users.All(u => u.FullName.EndsWith(" [Custom]")).ShouldBeTrue();
    }

    [Fact]
    public async Task ToTargetsAsync_WithStaticMapper_ShouldApplyCustomMapping()
    {
        // Arrange & Act
        var users = await MapperAsyncExtensions.ToTargetsAsync<User, TestUserDto, TestUserDtoStaticMapper>(_context.Set<User>()
            .Where(u => u.IsActive)
);

        // Assert
        users.ShouldNotBeNull();
        users.Count().ShouldBe(2);
        users.All(u => u.FullName.Contains(" ")).ShouldBeTrue();
        users.All(u => u.FullName.EndsWith(" [Static]")).ShouldBeTrue();
    }

    [Fact]
    public async Task ToTargetsAsync_WithInstanceMapper_ShouldAutoMapPropertiesFirst()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();

        // Act
        var users = await MapperAsyncExtensions.ToTargetsAsync(_context.Set<User>()
, mapper);

        // Assert
        users.Count().ShouldBe(3);
        users.All(u => !string.IsNullOrEmpty(u.FirstName)).ShouldBeTrue();
        users.All(u => !string.IsNullOrEmpty(u.Email)).ShouldBeTrue();
        users.All(u => u.Id > 0).ShouldBeTrue();
    }

    [Fact]
    public async Task FirstAsync_WithInstanceMapper_ShouldApplyCustomMapping()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();

        // Act
        var user = await _context.Set<User>()
            .Where(u => u.FirstName == "Alice")
            .FirstAsync<User, TestUserDto>(mapper);

        // Assert
        user.ShouldNotBeNull();
        user!.FirstName.ShouldBe("Alice");
        user.FullName.ShouldBe("Alice Johnson [Custom]");
    }

    [Fact]
    public async Task FirstAsync_WithStaticMapper_ShouldApplyCustomMapping()
    {
        // Arrange & Act
        var user = await _context.Set<User>()
            .Where(u => u.FirstName == "Bob")
            .FirstAsync<User, TestUserDto, TestUserDtoStaticMapper>();

        // Assert
        user.ShouldNotBeNull();
        user!.FirstName.ShouldBe("Bob");
        user.FullName.ShouldBe("Bob Smith [Static]");
    }

    [Fact]
    public async Task FirstAsync_WithInstanceMapper_WhenNoMatch_ShouldReturnNull()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();

        // Act
        var user = await _context.Set<User>()
            .Where(u => u.FirstName == "NonExistent")
            .FirstAsync<User, TestUserDto>(mapper);

        // Assert
        user.ShouldBeNull();
    }

    [Fact]
    public async Task FirstAsync_WithStaticMapper_WhenNoMatch_ShouldReturnNull()
    {
        // Arrange & Act
        var user = await _context.Set<User>()
            .Where(u => u.FirstName == "NonExistent")
            .FirstAsync<User, TestUserDto, TestUserDtoStaticMapper>();

        // Assert
        user.ShouldBeNull();
    }

    [Fact]
    public async Task SingleAsync_WithInstanceMapper_ShouldApplyCustomMapping()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();

        // Act
        var user = await _context.Set<User>()
            .Where(u => u.FirstName == "Charlie")
            .SingleAsync<User, TestUserDto>(mapper);

        // Assert
        user.ShouldNotBeNull();
        user.FirstName.ShouldBe("Charlie");
        user.FullName.ShouldBe("Charlie Brown [Custom]");
    }

    [Fact]
    public async Task SingleAsync_WithStaticMapper_ShouldApplyCustomMapping()
    {
        // Arrange & Act
        var user = await _context.Set<User>()
            .Where(u => u.FirstName == "Alice")
            .SingleAsync<User, TestUserDto, TestUserDtoStaticMapper>();

        // Assert
        user.ShouldNotBeNull();
        user.FirstName.ShouldBe("Alice");
        user.FullName.ShouldBe("Alice Johnson [Static]");
    }

    [Fact]
    public async Task SingleAsync_WithInstanceMapper_WhenMultipleMatches_ShouldThrow()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();

        // Act
        var act = async () => await _context.Set<User>()
            .Where(u => u.IsActive)
            .SingleAsync<User, TestUserDto>(mapper);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ToTargetsAsync_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        IMappingConfigurationAsyncInstance<User, TestUserDto> mapper = null!;

        // Act
        var act = async () => await MapperAsyncExtensions.ToTargetsAsync(_context.Set<User>()
, mapper);

        // Assert
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("mapper");
    }

    [Fact]
    public async Task FirstAsync_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        IMappingConfigurationAsyncInstance<User, TestUserDto> mapper = null!;

        // Act
        var act = async () => await _context.Set<User>()
            .FirstAsync<User, TestUserDto>(mapper);

        // Assert
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("mapper");
    }

    [Fact]
    public async Task SingleAsync_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        IMappingConfigurationAsyncInstance<User, TestUserDto> mapper = null!;

        // Act
        var act = async () => await _context.Set<User>()
            .SingleAsync<User, TestUserDto>(mapper);

        // Assert
        var ex = await act.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("mapper");
    }

    [Fact]
    public async Task ToTargetsAsync_WithInstanceMapper_ShouldRespectCancellationToken()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await MapperAsyncExtensions.ToTargetsAsync(_context.Set<User>()
, mapper, cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FirstAsync_WithInstanceMapper_ShouldRespectCancellationToken()
    {
        // Arrange
        var mapper = new TestUserDtoAsyncMapper();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _context.Set<User>()
            .FirstAsync<User, TestUserDto>(mapper, cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
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

[MappingTarget<User>( "Password", "CreatedAt")]
public partial class TestUserDto
{
    public string FullName { get; set; } = string.Empty;
}

public class TestUserDtoAsyncMapper : IMappingConfigurationAsyncInstance<User, TestUserDto>
{
    public Task MapAsync(User source, TestUserDto target, CancellationToken cancellationToken = default)
    {
        target.FullName = $"{source.FirstName} {source.LastName} [Custom]";
        return Task.CompletedTask;
    }
}

public class TestUserDtoStaticMapper : IMappingConfigurationAsync<User, TestUserDto>
{
    public static Task MapAsync(User source, TestUserDto target, CancellationToken cancellationToken = default)
    {
        target.FullName = $"{source.FirstName} {source.LastName} [Static]";
        return Task.CompletedTask;
    }
}
