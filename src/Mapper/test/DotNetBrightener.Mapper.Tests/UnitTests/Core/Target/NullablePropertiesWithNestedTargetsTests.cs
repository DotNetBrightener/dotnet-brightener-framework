namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

// Test entities that mimic EF Core entities with navigation properties
public class ChunkEmbedding1024
{
    public int Id { get; set; }
    public int ChunkIdFk { get; set; }
    public int ModelIdFk { get; set; }
    public float[] Embedding { get; set; } = [];

    public virtual Chunk ChunkIdFkNavigation { get; set; } = null!;
    public virtual EmbeddingModel ModelIdFkNavigation { get; set; } = null!;
}

public class Chunk
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int DocumentId { get; set; }
}

public class EmbeddingModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Dimensions { get; set; }
}

// Test entity with nullable nested navigation property
public class NullableWorkerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NullableCompanyEntity? Company { get; set; }
}

public class NullableCompanyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

// Test entity for collection nested targets with NullableProperties
public class NullableOrganizationEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<NullableEmployeeEntity> Employees { get; set; } = [];
}

public class NullableEmployeeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// Target DTOs - see GitHub issue #116
[MappingTarget<Chunk>( GenerateToSource = true)]
public partial class ChunkDto;

[MappingTarget<EmbeddingModel>( GenerateToSource = true)]
public partial class EmbeddingModelDto;

[MappingTarget<ChunkEmbedding1024>(
    nameof(ChunkEmbedding1024.ModelIdFkNavigation),
    NestedTargetTypes = [typeof(ChunkDto), typeof(EmbeddingModelDto)],
    NullableProperties = true,
    GenerateToSource = true)]
public partial class ChunkEmbedding1024Dto;

// Test targets for nested targets with nullable properties
[MappingTarget<NullableCompanyEntity>( NullableProperties = true, GenerateToSource = true)]
public partial class NullableCompanyTarget;

[MappingTarget<NullableWorkerEntity>(
    NestedTargetTypes = [typeof(NullableCompanyTarget)],
    NullableProperties = true,
    GenerateToSource = true)]
public partial class NullableWorkerTarget;

// Test targets for collection nested targets with nullable properties
[MappingTarget<NullableEmployeeEntity>( NullableProperties = true)]
public partial class NullableEmployeeTarget;

[MappingTarget<NullableOrganizationEntity>(
    NestedTargetTypes = [typeof(NullableEmployeeTarget)],
    NullableProperties = true)]
public partial class NullableOrganizationTarget;

public class NullablePropertiesWithNestedTargetsTests
{
    [Fact]
    public void NestedTarget_ShouldBeNullable_WhenNullablePropertiesIsTrue()
    {
        // Arrange & Act
        var dtoType = typeof(NullableWorkerTarget);

        // Assert
        var idProp = dtoType.GetProperty("Id");
        idProp.ShouldNotBeNull();
        idProp!.PropertyType.ShouldBe(typeof(int?), "Id should be nullable int");

        var nameProp = dtoType.GetProperty("Name");
        nameProp.ShouldNotBeNull();
        nameProp!.PropertyType.ShouldBe(typeof(string), "Name is a reference type");

        var companyProp = dtoType.GetProperty("Company");
        companyProp.ShouldNotBeNull();

        companyProp!.PropertyType.ShouldBe(typeof(NullableCompanyTarget),
            "Company nested target should be nullable reference type (NullableCompanyTarget?)");
    }

    [Fact]
    public void ChunkEmbedding1024Dto_ShouldHaveNullableNestedTargets_WhenNullablePropertiesIsTrue()
    {
        // Arrange & Act
        var dtoType = typeof(ChunkEmbedding1024Dto);

        // Assert - All properties should be nullable
        var idProp = dtoType.GetProperty("Id");
        idProp.ShouldNotBeNull();
        idProp!.PropertyType.ShouldBe(typeof(int?), "Id should be nullable int");

        var chunkIdFkProp = dtoType.GetProperty("ChunkIdFk");
        chunkIdFkProp.ShouldNotBeNull();
        chunkIdFkProp!.PropertyType.ShouldBe(typeof(int?), "ChunkIdFk should be nullable int");

        var modelIdFkProp = dtoType.GetProperty("ModelIdFk");
        modelIdFkProp.ShouldNotBeNull();
        modelIdFkProp!.PropertyType.ShouldBe(typeof(int?), "ModelIdFk should be nullable int");

        var chunkNavProp = dtoType.GetProperty("ChunkIdFkNavigation");
        chunkNavProp.ShouldNotBeNull();
        chunkNavProp!.PropertyType.ShouldBe(typeof(ChunkDto),
            "ChunkIdFkNavigation nested target should be nullable (ChunkDto?)");
    }

    [Fact]
    public void CollectionNestedTarget_ShouldBeNullable_WhenNullablePropertiesIsTrue()
    {
        // Arrange & Act
        var dtoType = typeof(NullableOrganizationTarget);

        // Assert
        var idProp = dtoType.GetProperty("Id");
        idProp.ShouldNotBeNull();
        idProp!.PropertyType.ShouldBe(typeof(int?), "Id should be nullable int");

        var employeesProp = dtoType.GetProperty("Employees");
        employeesProp.ShouldNotBeNull();
        employeesProp!.PropertyType.ShouldBe(typeof(List<NullableEmployeeTarget>),
            "Employees collection should be nullable (List<NullableEmployeeTarget>?)");
    }

    [Fact]
    public void Constructor_ShouldHandleNullNestedTarget_WithNullableProperties()
    {
        // Arrange
        var worker = new NullableWorkerEntity
        {
            Id = 1,
            Name = "John Doe",
            Company = null
        };

        // Act
        var dto = new NullableWorkerTarget(worker);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("John Doe");
        dto.Company.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldMapNonNullNestedTarget_WithNullableProperties()
    {
        // Arrange
        var worker = new NullableWorkerEntity
        {
            Id = 2,
            Name = "Jane Smith",
            Company = new NullableCompanyEntity
            {
                Id = 100,
                Name = "Acme Corp",
                Address = "123 Main St"
            }
        };

        // Act
        var dto = new NullableWorkerTarget(worker);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(2);
        dto.Name.ShouldBe("Jane Smith");
        dto.Company.ShouldNotBeNull();
        dto.Company!.Id.ShouldBe(100);
        dto.Company.Name.ShouldBe("Acme Corp");
        dto.Company.Address.ShouldBe("123 Main St");
    }

    [Fact]
    public void ToSource_ShouldHandleNullableProperties_WithoutCompilationErrors()
    {
        // Arrange
        var dto = new ChunkEmbedding1024Dto
        {
            Id = 1,
            ChunkIdFk = 10,
            ModelIdFk = 20,
            Embedding = [0.1f, 0.2f],
            ChunkIdFkNavigation = new ChunkDto
            {
                Id = 10,
                Content = "Test content",
                DocumentId = 5
            }
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(1);
        entity.ChunkIdFk.ShouldBe(10);
        entity.ModelIdFk.ShouldBe(20);
        entity.ChunkIdFkNavigation.ShouldNotBeNull();
        entity.ChunkIdFkNavigation.Id.ShouldBe(10);
        entity.ChunkIdFkNavigation.Content.ShouldBe("Test content");
    }

    [Fact]
    public void ToSource_ShouldHandleNullNestedTarget_WithNullableProperties()
    {
        // Arrange
        var dto = new NullableWorkerTarget
        {
            Id = 3,
            Name = "Bob Johnson",
            Company = null
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(3);
        entity.Name.ShouldBe("Bob Johnson");
        entity.Company.ShouldBeNull();
    }

    [Fact]
    public void ToSource_ShouldMapNonNullNestedTarget_WithNullableProperties()
    {
        // Arrange
        var dto = new NullableWorkerTarget
        {
            Id = 4,
            Name = "Alice Williams",
            Company = new NullableCompanyTarget
            {
                Id = 200,
                Name = "TechCo",
                Address = "456 Tech Ave"
            }
        };

        // Act
        var entity = dto.ToSource();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(4);
        entity.Name.ShouldBe("Alice Williams");
        entity.Company.ShouldNotBeNull();
        entity.Company!.Id.ShouldBe(200);
        entity.Company.Name.ShouldBe("TechCo");
        entity.Company.Address.ShouldBe("456 Tech Ave");
    }

    [Fact]
    public void Projection_ShouldHandleNullableNestedTarget_WithNullableProperties()
    {
        // Arrange
        var workers = new[]
        {
            new NullableWorkerEntity
            {
                Id = 1,
                Name = "Worker 1",
                Company = null
            },
            new NullableWorkerEntity
            {
                Id = 2,
                Name = "Worker 2",
                Company = new NullableCompanyEntity { Id = 100, Name = "Company A", Address = "Address A" }
            }
        }.AsQueryable();

        // Act
        var dtos = workers.Select(NullableWorkerTarget.Projection).ToList();

        // Assert
        dtos.Count().ShouldBe(2);
        dtos[0].Id.ShouldBe(1);
        dtos[0].Company.ShouldBeNull();
        dtos[1].Id.ShouldBe(2);
        dtos[1].Company.ShouldNotBeNull();
        dtos[1].Company!.Name.ShouldBe("Company A");
    }
}
