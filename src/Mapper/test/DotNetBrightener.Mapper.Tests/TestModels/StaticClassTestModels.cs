namespace DotNetBrightener.Mapper.Tests.TestModels;

public static class Foo
{
    public sealed record Bar
    {
        public string Name  { get; set; } = string.Empty;
        public int    Value { get; set; }

        public Arr? Arr1 { get; set; }

        public sealed class Arr { public int Length { get; set; } }
    }
}

[MappingTarget<Foo.Bar>()]
public sealed partial record BarDto;
