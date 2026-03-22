using DotNetBrightener.Mapper.Generators.Shared;

namespace DotNetBrightener.Mapper;

public static class SymbolNameExtensions
{
    public static string GetSafeName(this string symbol)
    {
        return GeneratorUtilities.StripGlobalPrefix(symbol)
            .Replace("<", "_")
            .Replace(">", "_");
    }
}
