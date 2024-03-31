using CRUD_With_gRPC.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;


namespace DotNetBrightener.gRPC.Generator.Tests;

public class ProtoFileGeneratorTests
{
    //private ServiceProvider _serviceProvider;

    //[SetUp]
    //public void Setup()
    //{
    //    var serviceCollection = new ServiceCollection();

    //    _serviceProvider = serviceCollection.BuildServiceProvider();

    //}

    //[TearDown]
    //public void TearDown()
    //{
    //    _serviceProvider.Dispose();
    //}

    [Test]
    public void Test()
    {
        // Create the 'input' compilation that the generator will act on
        string[] sources =
        [
            TestHelpers.AttributesSource,
            TestHelpers.PagedCollectionSource,
            @"
using CRUD_With_gRPC.Core;
using DotNetBrightener.gRPC;
using DotNetBrightener.GenericCRUD.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace gRPCWebDemo;

public interface IGrpcServicesProvider {
    List<Type> ServiceTypes { get; }
}

public class GrpcServicesProvider : IGrpcServicesProvider
{
    public List<Type> ServiceTypes { get; } =
    [
        typeof(IProductGrpcService)
    ];
}

public class Product
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsDeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }
}

public class ProductFilterQuery
{
    //public int PageSize { get; set; } = 50;

    //public int PageIndex { get; set; } = 0;

    //public string OrderBy { get; set; }

    //public string Columns { get; set; }

    //public bool DeletedRecordsOnly { get; set; } = false;

    public Dictionary<string, string> Filters { get; set; }
}

[GrpcService(Name = ""ProductService"")]
public interface IProductGrpcService
{
    [GrpcToRestApi(RouteTemplate = ""api/products"")]
    Task<PagedCollection<Product>> GetProductsList(ProductFilterQuery filterQuery);

    [GrpcToRestApi(Method = ""GET"", RouteTemplate = ""api/products/{id}"")]
    Task<Product> GetProduct(long id);
}
"
        ];

        var  inputCompilation = sources.CreateCompilation();

        TestGenerator generator = new TestGenerator();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        driver.GetRunResult();

        Assert.That(generator.CodeGeneratorInformation, Is.Not.Null);

        foreach (var generatedFile in generator.ProtoFiles)
        {
            Console.WriteLine("-------");
            Console.WriteLine(generatedFile.FilePath);
            Console.WriteLine(generatedFile.FileContent);
            Console.WriteLine();
        }

        foreach (var generatedFile in generator.ServiceFiles)
        {
            Console.WriteLine("-------");
            Console.WriteLine(generatedFile.FilePath);
            Console.WriteLine(generatedFile.FileContent);
            Console.WriteLine();
        }

        foreach (var generatedFile in generator.ServiceImplFiles)
        {
            Console.WriteLine("-------");
            Console.WriteLine(generatedFile.FilePath);
            Console.WriteLine(generatedFile.FileContent);
            Console.WriteLine();
        }

        foreach (var generatedFile in generator.MessageFiles)
        {
            Console.WriteLine("-------");
            Console.WriteLine(generatedFile.FilePath);
            Console.WriteLine(generatedFile.FileContent);
            Console.WriteLine();
        }
    }
}

internal static class StringSourceCompilationHelper
{
    public static Compilation CreateCompilation(this string source)
        => CSharpCompilation.Create("compilation",
                                    new[]
                                    {
                                        CSharpSyntaxTree.ParseText(source)
                                    },
                                    new[]
                                    {
                                        MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                                        MetadataReference.CreateFromFile(typeof(IProductGrpcService).GetTypeInfo().Assembly.Location),
                                    },
                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    public static Compilation CreateCompilation(this string[] sources)
        => CSharpCompilation.Create("compilation",
                                    sources.Select(source => CSharpSyntaxTree.ParseText(source)),
                                    new[]
                                    {
                                        MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                                        MetadataReference.CreateFromFile(typeof(IProductGrpcService).GetTypeInfo().Assembly.Location),
                                    },
                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}