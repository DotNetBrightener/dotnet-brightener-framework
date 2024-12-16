using BenchmarkDotNet.Attributes;

namespace DotNetBrightener.Uuidv7.Benchmarks;

public class BenchmarkJobs
{
    [Benchmark]
    public Guid Guidv7WithLibrary()
    {
        return Uuid7.Guid();
    }

    [Benchmark]
    public Guid Guidv7Creation()
    {
        var guid = Guid.CreateVersion7();

        return guid;
    }

}