using DotNetBrightener.WebApi.GenericCRUD.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.WebApi.GenericCRUD.Tests;

public class TestEntity
{
    [Key]
    public long Id { get; set; }


    [MaxLength(512)]
    public string Value { get; set; }

    public List<TestEntity> SubEntities { get; set; }

    public ICollection<TestEntity2> SubEntities2 { get; set; }

    [JsonIgnore]
    public string IgnoredProperty { get; set; }
}

public class TestEntity2
{
    [Key]
    public long Id { get; set; }

    public long TestEntityId { get; set; }

    [JsonIgnore]
    public TestEntity TestEntity { get; set; }

}

public class EntityMetadataExtractorTests
{
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();

    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public void GetDefaultColumns_ShouldReturnExpectedColumns()
    {
        var actualDetectedColumn = typeof(TestEntity).GetDefaultColumns();

        var defaultColumn2 = typeof(TestEntity2).GetDefaultColumns();

        var expectedColumns = new[]
        {
            nameof(TestEntity.Id),
            nameof(TestEntity.Value),
            nameof(TestEntity.SubEntities),
            nameof(TestEntity.SubEntities2),
            $"{nameof(TestEntity.SubEntities)}.{nameof(TestEntity.Id)}",
            $"{nameof(TestEntity.SubEntities)}.{nameof(TestEntity.Value)}",
            $"{nameof(TestEntity.SubEntities)}.{nameof(TestEntity.SubEntities)}",
            $"{nameof(TestEntity.SubEntities)}.{nameof(TestEntity.SubEntities2)}",
            $"{nameof(TestEntity.SubEntities2)}.{nameof(TestEntity.Id)}",
            $"{nameof(TestEntity.SubEntities2)}.{nameof(TestEntity2.TestEntityId)}",
        };



        Assert.That(actualDetectedColumn.Length, Is.EqualTo(expectedColumns.Length));

        Assert.That(actualDetectedColumn.All(c => expectedColumns.Contains(c)), Is.True);

        Console.WriteLine("Actual columns 1: {0}\r\n\r\n", string.Join(", ", actualDetectedColumn));
    }

    [Test]
    public void GetDefaultColumns_ShouldReturnExpectedColumns2()
    {
        var actualDetectedColumn = typeof(TestEntity).GetDefaultColumns();
        
        var defaultColumn2 = typeof(TestEntity2).GetDefaultColumns();


        var expectedColumns2 = new[]
        {
            nameof(TestEntity2.Id),
            nameof(TestEntity2.TestEntityId),
            $"{nameof(TestEntity2.TestEntity)}.{nameof(TestEntity.Id)}",
            $"{nameof(TestEntity2.TestEntity)}.{nameof(TestEntity.Value)}",
            $"{nameof(TestEntity2.TestEntity)}.{nameof(TestEntity.SubEntities)}",
            $"{nameof(TestEntity2.TestEntity)}.{nameof(TestEntity.SubEntities2)}",
        };


        Assert.That(defaultColumn2.Length, Is.EqualTo(expectedColumns2.Length));

        Assert.That(defaultColumn2.All(c => expectedColumns2.Contains(c)), Is.True);

        Console.WriteLine("Actual columns 2: {0}", string.Join(", ", defaultColumn2));
    }
}