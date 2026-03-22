namespace DotNetBrightener.Mapper.Tests.TestModels;

// Models with circular references
public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Book> Books { get; set; } = [];
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Author? Author { get; set; }
}

// Self-referencing model
public class OrgEmployee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrgEmployee? Manager { get; set; }
    public List<OrgEmployee> DirectReports { get; set; } = [];
}

// Targets with MaxDepth for depth limiting (without reference tracking)
[MappingTarget<Author>( MaxDepth = 2, PreserveReferences = false, NestedTargetTypes = [typeof(BookTargetWithDepth)], GenerateToSource = true)]
public partial record AuthorTargetWithDepth;

[MappingTarget<Book>( MaxDepth = 2, PreserveReferences = false, NestedTargetTypes = [typeof(AuthorTargetWithDepth)], GenerateToSource = true)]
public partial record BookTargetWithDepth;

// Targets with PreserveReferences for runtime tracking (also needs MaxDepth to prevent generator SO)
[MappingTarget<Author>( MaxDepth = 3, PreserveReferences = true, NestedTargetTypes = [typeof(BookTargetWithTracking)])]
public partial record AuthorTargetWithTracking;

[MappingTarget<Book>( MaxDepth = 3, PreserveReferences = true, NestedTargetTypes = [typeof(AuthorTargetWithTracking)])]
public partial record BookTargetWithTracking;

// Self-referencing target with both MaxDepth and PreserveReferences
[MappingTarget<OrgEmployee>( MaxDepth = 5, PreserveReferences = true, NestedTargetTypes = [typeof(OrgEmployeeTarget)])]
public partial record OrgEmployeeTarget;
