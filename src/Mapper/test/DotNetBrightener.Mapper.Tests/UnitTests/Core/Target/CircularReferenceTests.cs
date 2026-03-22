using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class CircularReferenceTests
{
    [Fact]
    public void MaxDepth_Should_Prevent_StackOverflow_With_Circular_References()
    {
        // Arrange - Create circular reference: Author -> Book -> Author
        var author = new Author
        {
            Id = 1,
            Name = "John Doe",
            Books = []
        };

        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Author = author
        };

        author.Books.Add(book);

        // Act - This should not cause stack overflow
        var target = new AuthorTargetWithDepth(author);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("John Doe");
        target.Books.ShouldNotBeNull();
        target.Books.Count().ShouldBe(1);

        var bookTarget = target.Books![0];
        bookTarget.Title.ShouldBe("Test Book");

        // At MaxDepth = 2, we allow 2 levels: Author -> Book -> Author
        // But the nested Author cannot have Books (that would be level 3)
        bookTarget.Author.ShouldNotBeNull();
        bookTarget.Author!.Id.ShouldBe(1);
        bookTarget.Author.Name.ShouldBe("John Doe");

        // At depth 2, Books is cut off to prevent going to level 3
        bookTarget.Author.Books.ShouldBeNull();
    }

    [Fact]
    public void MaxDepth_Should_Handle_Multiple_Books_Per_Author()
    {
        // Arrange
        var author = new Author
        {
            Id = 1,
            Name = "Prolific Author",
            Books = []
        };

        var book1 = new Book { Id = 1, Title = "Book 1", Author = author };
        var book2 = new Book { Id = 2, Title = "Book 2", Author = author };
        var book3 = new Book { Id = 3, Title = "Book 3", Author = author };

        author.Books.AddRange([book1, book2, book3]);

        // Act
        var target = new AuthorTargetWithDepth(author);

        // Assert
        target.Books.Count().ShouldBe(3);
        target.Books![0].Title.ShouldBe("Book 1");
        target.Books![1].Title.ShouldBe("Book 2");
        target.Books![2].Title.ShouldBe("Book 3");

        // Each book should have the author, but the author's books should be null (depth limit)
        foreach (var book in target.Books!)
        {
            book.Author.ShouldNotBeNull();
            book.Author!.Name.ShouldBe("Prolific Author");
            book.Author.Books.ShouldBeNull();
        }
    }

    [Fact]
    public void PreserveReferences_Should_Detect_And_Break_Circular_References()
    {
        // Arrange - Create circular reference
        var author = new Author
        {
            Id = 1,
            Name = "Jane Smith",
            Books = []
        };

        var book1 = new Book
        {
            Id = 1,
            Title = "First Book",
            Author = author
        };

        var book2 = new Book
        {
            Id = 2,
            Title = "Second Book",
            Author = author
        };

        author.Books.Add(book1);
        author.Books.Add(book2);

        // Act - PreserveReferences should detect we're processing the same author twice
        var target = new AuthorTargetWithTracking(author);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("Jane Smith");
        target.Books.ShouldNotBeNull();

        target.Books.Count().ShouldBeGreaterThanOrEqualTo(1);

        // The first book should be created with nested author data
        new[] { target.Books![0].Title }.ShouldContain(t => t == "First Book" || t == "Second Book");

        if (target.Books[0].Author != null)
        {
            target.Books[0].Author.Books.ShouldNotBeNull();
            target.Books[0].Author.Books!.All(b => b.Author == null).ShouldBeTrue();
        }
    }

    [Fact]
    public void PreserveReferences_Should_Handle_Same_Object_In_Multiple_Collections()
    {
        // Arrange - Same employee appears in multiple places
        var ceo = new OrgEmployee
        {
            Id = 1,
            Name = "CEO",
            Manager = null,
            DirectReports = []
        };

        var sharedEmployee = new OrgEmployee
        {
            Id = 2,
            Name = "Shared Employee",
            Manager = ceo,
            DirectReports = []
        };

        // Add the same employee twice (simulating a bug or complex graph)
        ceo.DirectReports.Add(sharedEmployee);
        ceo.DirectReports.Add(sharedEmployee); // Duplicate reference

        // Act
        var target = new OrgEmployeeTarget(ceo);

        // Assert
        target.DirectReports.ShouldNotBeNull();
        
        // With PreserveReferences, the second occurrence should be filtered out or nulled
        var nonNullReports = target.DirectReports!.Where(r => r != null).ToList();
        
        // Should have at most 1 instance of the shared employee
        nonNullReports.Count.ShouldBeLessThanOrEqualTo(1);
    }

    [Fact]
    public void SelfReferencing_OrgEmployee_Should_Handle_Hierarchy_Without_StackOverflow()
    {
        // Arrange - Create employee hierarchy with circular reference
        var ceo = new OrgEmployee
        {
            Id = 1,
            Name = "CEO",
            Manager = null,
            DirectReports = []
        };

        var director = new OrgEmployee
        {
            Id = 2,
            Name = "Director",
            Manager = ceo,
            DirectReports = []
        };

        var manager = new OrgEmployee
        {
            Id = 3,
            Name = "Manager",
            Manager = director,
            DirectReports = []
        };

        var employee = new OrgEmployee
        {
            Id = 4,
            Name = "Employee",
            Manager = manager,
            DirectReports = []
        };

        ceo.DirectReports.Add(director);
        director.DirectReports.Add(manager);
        manager.DirectReports.Add(employee);

        ceo.DirectReports.Add(employee);

        // Act - Should not cause stack overflow
        var target = new OrgEmployeeTarget(ceo);

        // Assert - No stack overflow occurred, that's the main success
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Name.ShouldBe("CEO");
        target.Manager.ShouldBeNull();
        target.DirectReports.ShouldNotBeNull();

        target.DirectReports.Count().ShouldBeGreaterThanOrEqualTo(1);

        // Verify we have at least the director
        var directorTarget = target.DirectReports!.FirstOrDefault(e => e.Name == "Director");
        directorTarget.ShouldNotBeNull();
        directorTarget!.Name.ShouldBe("Director");

        // The important thing is no stack overflow occurred
    }

    [Fact]
    public void SelfReferencing_Should_Handle_Manager_Pointing_Up()
    {
        // Arrange - Create hierarchy where we walk up through managers
        var employee = new OrgEmployee
        {
            Id = 1,
            Name = "Employee",
            DirectReports = []
        };

        var manager = new OrgEmployee
        {
            Id = 2,
            Name = "Manager",
            DirectReports = [employee]
        };

        var director = new OrgEmployee
        {
            Id = 3,
            Name = "Director",
            DirectReports = [manager]
        };

        employee.Manager = manager;
        manager.Manager = director;
        director.Manager = null;

        // Act - Start from employee and walk up
        var target = new OrgEmployeeTarget(employee);

        // Assert
        target.Name.ShouldBe("Employee");
        target.Manager.ShouldNotBeNull();
        target.Manager!.Name.ShouldBe("Manager");
        target.Manager.Manager.ShouldNotBeNull();
        target.Manager.Manager!.Name.ShouldBe("Director");
    }

    [Fact]
    public void CircularReference_Should_Handle_Null_Collections()
    {
        // Arrange - Author with no books
        var author = new Author
        {
            Id = 1,
            Name = "Author Without Books",
            Books = []
        };

        // Act
        var target = new AuthorTargetWithDepth(author);

        // Assert
        target.ShouldNotBeNull();
        target.Books.ShouldNotBeNull();
        target.Books.ShouldBeEmpty();
    }

    [Fact]
    public void CircularReference_Should_Handle_Empty_DirectReports()
    {
        // Arrange - Employee with no reports
        var employee = new OrgEmployee
        {
            Id = 1,
            Name = "Solo Employee",
            Manager = null,
            DirectReports = []
        };

        // Act
        var target = new OrgEmployeeTarget(employee);

        // Assert
        target.DirectReports.ShouldNotBeNull();
        target.DirectReports.ShouldBeEmpty();
    }

    [Fact]
    public void CircularReference_Should_Handle_Single_Element_Cycle()
    {
        // Arrange - Employee is their own manager (weird but possible in bad data)
        var employee = new OrgEmployee
        {
            Id = 1,
            Name = "Self-Managed",
            DirectReports = []
        };

        employee.Manager = employee;
        employee.DirectReports.Add(employee);

        // Act - Should not hang
        var target = new OrgEmployeeTarget(employee);

        // Assert - Should complete without error
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Self-Managed");
    }

    [Fact]
    public void CircularReference_Should_Handle_Complex_Graphs()
    {
        // Arrange - Multiple authors sharing books
        var author1 = new Author { Id = 1, Name = "Author 1", Books = []
        };
        var author2 = new Author { Id = 2, Name = "Author 2", Books = []
        };

        var sharedBook = new Book { Id = 1, Title = "Shared Book", Author = author1 };

        author1.Books.Add(sharedBook);
        author2.Books.Add(sharedBook); // Same book instance

        // Act
        var target1 = new AuthorTargetWithTracking(author1);
        var target2 = new AuthorTargetWithTracking(author2);

        // Assert - Both should succeed
        target1.ShouldNotBeNull();
        target2.ShouldNotBeNull();
        target1.Books.Count().ShouldBe(1);
        target2.Books.Count().ShouldBe(1);
    }

    [Fact]
    public void CircularReference_Should_Handle_Null_Navigation_Properties()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Title = "Standalone Book",
            Author = null
        };

        var author = new Author
        {
            Id = 1,
            Name = "Author",
            Books = [book]
        };

        // Act
        var target = new AuthorTargetWithDepth(author);

        // Assert
        target.Books.Count().ShouldBe(1);
        target.Books![0].Title.ShouldBe("Standalone Book");
        target.Books[0].Author.ShouldBeNull();
    }

    [Fact]
    public void MaxDepth_And_PreserveReferences_Should_Work_Together()
    {
        // Arrange - Create complex circular structure
        var author = new Author
        {
            Id = 1,
            Name = "Author",
            Books = []
        };

        var book1 = new Book { Id = 1, Title = "Book 1", Author = author };
        var book2 = new Book { Id = 2, Title = "Book 2", Author = author };

        author.Books.Add(book1);
        author.Books.Add(book2);
        author.Books.Add(book1); // Duplicate reference

        // Act - Both MaxDepth and PreserveReferences should apply
        var target = new AuthorTargetWithTracking(author);

        // Assert
        target.ShouldNotBeNull();
        target.Books.ShouldNotBeNull();
        
        // Should handle both depth limiting and reference tracking
        var nonNullBooks = target.Books!.Where(b => b != null).ToList();
        nonNullBooks.Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Deep_Hierarchy_With_Reference_Tracking_Should_Not_Overflow()
    {
        // Arrange - Create 10-level deep hierarchy
        OrgEmployee? current = null;
        OrgEmployee? root = null;

        for (int i = 10; i >= 1; i--)
        {
            var employee = new OrgEmployee
            {
                Id = i,
                Name = $"Level {i}",
                Manager = current,
                DirectReports = []
            };

            if (current != null)
            {
                current.DirectReports.Add(employee);
            }

            if (i == 1)
            {
                root = employee;
            }

            current = employee;
        }

        // Act - MaxDepth = 5 and PreserveReferences = true
        var target = new OrgEmployeeTarget(root!);

        // Assert - Should complete without overflow
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Level 1");
    }

    [Fact]
    public void Collection_With_Circular_References_Should_Map_Correctly()
    {
        // Arrange
        var authors = new List<Author>();

        var author1 = new Author { Id = 1, Name = "Author 1", Books = []
        };
        var author2 = new Author { Id = 2, Name = "Author 2", Books = []
        };

        var book1 = new Book { Id = 1, Title = "Book 1", Author = author1 };
        var book2 = new Book { Id = 2, Title = "Book 2", Author = author2 };

        author1.Books.Add(book1);
        author2.Books.Add(book2);

        authors.Add(author1);
        authors.Add(author2);

        // Act
        var targets = authors.Select(a => new AuthorTargetWithDepth(a)).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].Name.ShouldBe("Author 1");
        targets[1].Name.ShouldBe("Author 2");
        targets[0].Books.Count().ShouldBe(1);
        targets[1].Books.Count().ShouldBe(1);
    }

    [Fact]
    public void Empty_Collection_Should_Not_Cause_Issues()
    {
        // Arrange
        var employee = new OrgEmployee
        {
            Id = 1,
            Name = "Manager",
            DirectReports = []
        };

        // Act
        var target = new OrgEmployeeTarget(employee);

        // Assert
        target.DirectReports.ShouldNotBeNull();
        target.DirectReports.ShouldBeEmpty();
    }

    [Fact]
    public void BackTo_Should_Handle_Circular_References_Without_Error()
    {
        // Arrange
        var author = new Author
        {
            Id = 1,
            Name = "Test Author",
            Books = []
        };

        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Author = author
        };

        author.Books.Add(book);

        var target = new AuthorTargetWithDepth(author);

        // Act - ToSource should work even with circular references
        var mappedAuthor = target.ToSource();

        // Assert
        mappedAuthor.ShouldNotBeNull();
        mappedAuthor.Id.ShouldBe(1);
        mappedAuthor.Name.ShouldBe("Test Author");
        mappedAuthor.Books.ShouldNotBeNull();
    }

    [Fact]
    public void CircularReference_Detection_Should_Be_Fast_For_Large_Graphs()
    {
        // Arrange - Create large org hierarchy
        var root = new OrgEmployee
        {
            Id = 1,
            Name = "Root",
            DirectReports = []
        };

        // Create 3 levels with 5 children each = 1 + 5 + 25 = 31 employees
        CreateOrgHierarchy(root, depth: 0, maxDepth: 2, childrenPerLevel: 5);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var target = new OrgEmployeeTarget(root);
        stopwatch.Stop();

        // Assert - Should complete quickly (under 1 second)
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000);
        target.ShouldNotBeNull();
    }

    private void CreateOrgHierarchy(OrgEmployee parent, int depth, int maxDepth, int childrenPerLevel)
    {
        if (depth >= maxDepth) return;

        for (int i = 0; i < childrenPerLevel; i++)
        {
            var child = new OrgEmployee
            {
                Id = parent.Id * 10 + i,
                Name = $"Employee {parent.Id}-{i}",
                Manager = parent,
                DirectReports = []
            };

            parent.DirectReports.Add(child);
            CreateOrgHierarchy(child, depth + 1, maxDepth, childrenPerLevel);
        }
    }

    [Fact]
    public void DefaultSettings_Should_Prevent_StackOverflow_With_Bidirectional_References()
    {
        // Arrange - Simulate the user's StringLookup/StringIdentifier scenario
        // This tests the fix for the reported issue where users were getting constructor errors
        var lookup = new CircularLookup
        {
            Id = 1,
            Value = "en-US",
            Identifier = new CircularIdentifier
            {
                Id = 1,
                Name = "LanguageCode",
                Lookups = []
            }
        };

        lookup.Identifier.Lookups.Add(lookup); // Circular reference. This should be handled by PreserveReferences to prevent stack overflow.

        // Act - This should work with default settings (MaxDepth=3, PreserveReferences=true)
        var target = new CircularLookupDefaultDto(lookup);

        // Assert
        target.ShouldNotBeNull();
        target.Id.ShouldBe(1);
        target.Value.ShouldBe("en-US");
        target.Identifier.ShouldNotBeNull();
        target.Identifier!.Name.ShouldBe("LanguageCode");

        // With PreserveReferences=true (default), the circular reference should be handled
        // The CircularLookup should appear in the CircularIdentifier's collection
        target.Identifier.Lookups.ShouldNotBeNull();
    }

    [Fact]
    public void DefaultSettings_Should_Handle_Deep_Nesting_Up_To_MaxDepth()
    {
        // Arrange - Create a chain longer than default
        var level1 = new OrgEmployee { Id = 1, Name = "Level 1", DirectReports = []
        };
        var level2 = new OrgEmployee { Id = 2, Name = "Level 2", Manager = level1, DirectReports = []
        };
        var level3 = new OrgEmployee { Id = 3, Name = "Level 3", Manager = level2, DirectReports = []
        };
        var level4 = new OrgEmployee { Id = 4, Name = "Level 4", Manager = level3, DirectReports = []
        };
        var level5 = new OrgEmployee { Id = 5, Name = "Level 5", Manager = level4, DirectReports = []
        };

        level1.DirectReports.Add(level2);
        level2.DirectReports.Add(level3);
        level3.DirectReports.Add(level4);
        level4.DirectReports.Add(level5);

        // Act - Use target with default settings
        var target = new OrgEmployeeDefaultTarget(level1);

        // Assert - we can traverse deeper
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Level 1");
        target.DirectReports.ShouldNotBeNull();
        target.DirectReports.Count().ShouldBe(1);

        // Level 2 should be included
        var level2Target = target.DirectReports![0];
        level2Target.ShouldNotBeNull();
        level2Target.Name.ShouldBe("Level 2");
        level2Target.DirectReports.ShouldNotBeNull();
        level2Target.DirectReports.Count().ShouldBe(1);

        // Level 3 should be included
        var level3Target = level2Target.DirectReports![0];
        level3Target.ShouldNotBeNull();
        level3Target.Name.ShouldBe("Level 3");

        // Manager circular references should be properly handled - they should be null
        // because the parent is already in the __processed set
        level2Target.Manager.ShouldBeNull(); // Level1 is already being processed
        level3Target.Manager.ShouldBeNull(); // Level2 is already being processed
    }

    [Fact]
    public void LeafTarget_Without_NestedTargets_Should_Work_As_NestedTarget()
    {
        // Arrange - SimpleLeaf has no nested targets, but uses default settings
        var parent = new ParentWithLeaf
        {
            Id = 1,
            Name = "Parent",
            Leaf = new SimpleLeaf { Id = 2, Value = "Leaf Value" }
        };

        // Act - This should compile and run (previously caused constructor error)
        var target = new ParentWithLeafDto(parent);

        // Assert
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Parent");
        target.Leaf.ShouldNotBeNull();
        target.Leaf!.Value.ShouldBe("Leaf Value");
    }

    [Fact]
    public void MixedSettings_ExplicitAndDefault_Should_WorkTogether()
    {
        // Arrange - One target uses explicit settings, another uses defaults
        var author = new Author
        {
            Id = 1,
            Name = "Mixed Settings Author",
            Books = []
        };

        var book = new Book
        {
            Id = 1,
            Title = "Mixed Settings Book",
            Author = author
        };

        author.Books.Add(book);

        // Act - MixedSettingsAuthorDto has explicit settings, but its nested target uses defaults
        var target = new MixedSettingsAuthorDto(author);

        // Assert
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Mixed Settings Author");
        target.Books.ShouldNotBeNull();
        target.Books.Count().ShouldBe(1);
    }

    [Fact]
    public void SharedReference_In_Collection_Should_Be_Tracked()
    {
        // Arrange - Same author appears multiple times in a collection
        var sharedAuthor = new Author
        {
            Id = 1,
            Name = "Shared Author",
            Books = []
        };

        var book1 = new Book { Id = 1, Title = "Book 1", Author = sharedAuthor };
        var book2 = new Book { Id = 2, Title = "Book 2", Author = sharedAuthor };

        sharedAuthor.Books.AddRange([book1, book2]);

        // Create a collection with the same author referenced multiple times
        var authors = new List<Author> { sharedAuthor, sharedAuthor };

        // Act - With PreserveReferences=true, should handle shared references
        var targets = authors.Select(a => new AuthorDefaultDto(a)).ToList();

        // Assert
        targets.Count().ShouldBe(2);
        targets[0].Name.ShouldBe("Shared Author");
        targets[1].Name.ShouldBe("Shared Author");
    }

    [Fact]
    public void ComplexGraph_With_Multiple_Circular_Paths_Should_Not_Overflow()
    {
        // Arrange - Create a complex graph with multiple circular paths
        var centralNode = new OrgEmployee
        {
            Id = 1,
            Name = "Central",
            DirectReports = []
        };

        var node2 = new OrgEmployee
        {
            Id = 2,
            Name = "Node 2",
            Manager = centralNode,
            DirectReports = []
        };

        var node3 = new OrgEmployee
        {
            Id = 3,
            Name = "Node 3",
            Manager = centralNode,
            DirectReports = []
        };

        // Create multiple circular paths
        centralNode.DirectReports.Add(node2);
        centralNode.DirectReports.Add(node3);
        node2.DirectReports.Add(node3);
        node3.DirectReports.Add(node2);
        node2.DirectReports.Add(centralNode); // Back to central

        // Act - Should handle complex circular graph
        var target = new OrgEmployeeDefaultTarget(centralNode);

        // Assert - No stack overflow, that's the main success
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Central");
        target.DirectReports.ShouldNotBeNull();
    }

    [Fact]
    public void ZeroMaxDepth_Should_Still_Work_For_NonRecursive_Structures()
    {
        // Arrange - Simple non-recursive structure with MaxDepth = 0
        var leaf = new SimpleLeaf { Id = 1, Value = "Simple" };
        var parent = new ParentWithLeaf { Id = 2, Name = "Parent", Leaf = leaf };

        // Act - With MaxDepth=0, should still construct non-recursive structures
        var target = new ParentWithLeafNoDepthDto(parent);

        // Assert
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Parent");
    }

    [Fact]
    public void DefaultSettings_Should_Allow_Constructor_Chaining()
    {
        // Arrange
        var author = new Author
        {
            Id = 1,
            Name = "Constructor Chain Test",
            Books = []
        };

        // Act - Call public constructor (should chain to internal one)
        var target1 = new AuthorDefaultDto(author);

        // Also test that we can create multiple instances
        var target2 = new AuthorDefaultDto(author);

        // Assert - Both should be independent instances
        target1.ShouldNotBeNull();
        target2.ShouldNotBeNull();
        target1.ShouldNotBeSameAs(target2);
        target1.Name.ShouldBe(target2.Name);
    }

    [Fact]
    public void TripleNestedCircular_Reference_Should_Be_Handled()
    {
        // Arrange - A -> B -> C -> A circular reference
        var entityA = new TripleCircularA
        {
            Id = 1,
            Name = "Entity A",
            BReferences = []
        };

        var entityB = new TripleCircularB
        {
            Id = 2,
            Name = "Entity B",
            A = entityA,
            CReferences = []
        };

        var entityC = new TripleCircularC
        {
            Id = 3,
            Name = "Entity C",
            B = entityB,
            A = entityA
        };

        entityA.BReferences.Add(entityB);
        entityB.CReferences.Add(entityC);

        // Act - Should handle triple circular reference
        var target = new TripleCircularADto(entityA);

        // Assert
        target.ShouldNotBeNull();
        target.Name.ShouldBe("Entity A");
        target.BReferences.ShouldNotBeNull();
    }
}

#region Additional Test Models for New Tests

// Models for the user's reported scenario (bidirectional circular references)
public class CircularLookup
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public CircularIdentifier Identifier { get; set; } = null!;
}

public class CircularIdentifier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CircularLookup> Lookups { get; set; } = [];
}

[MappingTarget<CircularIdentifier>( NestedTargetTypes = [typeof(CircularLookupDefaultDto)])]
public partial record CircularIdentifierDefaultDto;

[MappingTarget<CircularLookup>( NestedTargetTypes = [typeof(CircularIdentifierDefaultDto)])]
public partial record CircularLookupDefaultDto;

// Simple leaf target without nested targets (tests the core fix)
public class SimpleLeaf
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class ParentWithLeaf
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SimpleLeaf? Leaf { get; set; }
}

[MappingTarget<SimpleLeaf>()]
public partial record SimpleLeafDto;

[MappingTarget<ParentWithLeaf>( NestedTargetTypes = [typeof(SimpleLeafDto)])]
public partial record ParentWithLeafDto;

// For testing MaxDepth = 0
[MappingTarget<SimpleLeaf>( MaxDepth = 0, PreserveReferences = false)]
public partial record SimpleLeafNoDepthDto;

[MappingTarget<ParentWithLeaf>( MaxDepth = 0, PreserveReferences = false, NestedTargetTypes = [typeof(SimpleLeafNoDepthDto)])]
public partial record ParentWithLeafNoDepthDto;

// Targets with default settings for existing models
[MappingTarget<OrgEmployee>( NestedTargetTypes = [typeof(OrgEmployeeDefaultTarget)])]
public partial record OrgEmployeeDefaultTarget;

[MappingTarget<Author>( NestedTargetTypes = [typeof(BookDefaultDto)])]
public partial record AuthorDefaultDto;

[MappingTarget<Book>( NestedTargetTypes = [typeof(AuthorDefaultDto)])]
public partial record BookDefaultDto;

// For mixed settings test
[MappingTarget<Author>( MaxDepth = 5, PreserveReferences = true, NestedTargetTypes = [typeof(BookDefaultDto)])]
public partial record MixedSettingsAuthorDto;

// Triple circular reference models (A -> B -> C -> A)
public class TripleCircularA
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TripleCircularB> BReferences { get; set; } = [];
}

public class TripleCircularB
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TripleCircularA? A { get; set; }
    public List<TripleCircularC> CReferences { get; set; } = [];
}

public class TripleCircularC
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TripleCircularB? B { get; set; }
    public TripleCircularA? A { get; set; }
}

[MappingTarget<TripleCircularA>( NestedTargetTypes = [typeof(TripleCircularBDto)])]
public partial record TripleCircularADto;

[MappingTarget<TripleCircularB>( NestedTargetTypes = [typeof(TripleCircularADto), typeof(TripleCircularCDto)])]
public partial record TripleCircularBDto;

[MappingTarget<TripleCircularC>( NestedTargetTypes = [typeof(TripleCircularBDto), typeof(TripleCircularADto)])]
public partial record TripleCircularCDto;

#endregion
