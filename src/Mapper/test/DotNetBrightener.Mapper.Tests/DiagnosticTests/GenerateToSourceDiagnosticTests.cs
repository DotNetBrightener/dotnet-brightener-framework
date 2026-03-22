namespace DotNetBrightener.Mapper.Tests.DiagnosticTests;

// Test models to verify FAC023 diagnostic for GenerateToSource

// Source class with readonly properties (no setters)
public class SourceWithReadOnlyProperties
{
    public int Id { get; }
    public string Name { get; }
    
    public SourceWithReadOnlyProperties(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

// This should trigger FAC023 warning because source has no setters
[MappingTarget<SourceWithReadOnlyProperties>( GenerateToSource = true)]
public partial class TargetWithNoSetters;

// Source class with private constructor
public class SourceWithPrivateConstructor
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    private SourceWithPrivateConstructor()
    {
        Id = 0;
        Name = string.Empty;
    }
    
    public static SourceWithPrivateConstructor Create() => new SourceWithPrivateConstructor();
}

// This should trigger FAC023 warning because source has private constructor
[MappingTarget<SourceWithPrivateConstructor>( GenerateToSource = true)]
public partial class TargetWithPrivateConstructor;

// Source class with private setters
public class SourceWithPrivateSetters
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    
    public SourceWithPrivateSetters()
    {
        Id = 0;
        Name = string.Empty;
    }
}

// This should trigger FAC023 warning because source has private setters
[MappingTarget<SourceWithPrivateSetters>( GenerateToSource = true)]
public partial class TargetWithPrivateSetters;

// Source class with valid properties (should not trigger warning)
public class SourceWithPublicSetters
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public SourceWithPublicSetters()
    {
        Id = 0;
        Name = string.Empty;
    }
}

// This should NOT trigger FAC023 warning - source has public setters and parameterless constructor
[MappingTarget<SourceWithPublicSetters>( GenerateToSource = true)]
public partial class TargetWithPublicSetters;

// Source record with positional constructor (should not trigger warning)
public record SourceRecord(int Id, string Name);

// This should NOT trigger FAC023 warning - positional records are supported
[MappingTarget<SourceRecord>( GenerateToSource = true)]
public partial class TargetFromRecord;

// Source class with no constructor (implicit parameterless constructor)
public class SourceWithImplicitConstructor
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// This should NOT trigger FAC023 warning - implicit parameterless constructor exists
[MappingTarget<SourceWithImplicitConstructor>( GenerateToSource = true)]
public partial class TargetWithImplicitConstructor;
