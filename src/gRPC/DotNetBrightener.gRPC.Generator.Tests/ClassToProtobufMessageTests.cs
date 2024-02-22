using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetBrightener.gRPC.Generator.Tests;

public class ClassToProtobufMessageTests
{
    [Test]
    public void ClasDefinitionToProtobufMessage_ShouldGiveProperMessageDefinition()
    {
        // Arrange
        var classDefinition = @"
using System;
using System.Collections.Generic;

namespace SomeNameSpace;

public class ProductPagedCollection
{
    public IEnumerable<Product> Items { get; set; }

    public int TotalCount { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
    
    public int ResultCount { get; set; }
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
";

        // Act

        // Expectation
        var expectedResult = $@"
message ProductPagedCollectionMessage {{
    repeated ProductMessage items = 1;
    int32 total_count = 2;
    int32 page_index = 3;
    int32 page_size = 4;
    int32 result_count = 5;
}}

message ProductMessage {{
    int64 id = 1;
    string name = 2;
    string description = 3;
    float price = 4;
    optional google.protobuf.Timestamp created_date = 5;
    optional google.protobuf.Timestamp modified_date = 6;
    bool is_deleted = 7;
    optional string created_by = 8;
    optional string modified_by = 9;
}}";

        // Act
        var syntaxTree = CSharpSyntaxTree.ParseText(classDefinition);
        var root       = syntaxTree.GetCompilationUnitRoot();

        // Assert
        var classDeclarations = root.DescendantNodes()
                                    .OfType<ClassDeclarationSyntax>();

    }
}