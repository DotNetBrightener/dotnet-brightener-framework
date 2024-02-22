using DotNetBrightener.gRPC.Generator.Generators;
using Microsoft.CodeAnalysis;

namespace DotNetBrightener.gRPC.Generator.Tests;

[Generator]
public class TestGenerator : ProtoFileGenerator
{
    protected override bool ShouldWriteFile => false;
}