using System.Linq;
using System.Text;
using DotNetBrightener.Mapper.Generators.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotNetBrightener.Mapper.Generators.MappingTargetGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class TargetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var genericTargets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.GenericMappingAttributeFullName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, token) => ModelBuilder.BuildModel(ctx, token))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(genericTargets.Collect(), static (spc, models) =>
        {
            spc.CancellationToken.ThrowIfCancellationRequested();

            // Build a lookup dictionary for nested target resolution
            var targetLookup = models
                .Where(m => m is not null)
                .ToDictionary(m => m!.FullName, m => m!);

            // Generate code for each target with access to all target models
            foreach (var model in models)
            {
                if (model is null) continue;

                var code = CodeBuilder.Generate(model, targetLookup);
                spc.AddSource($"{model.FullName}.g.cs", SourceText.From(code, Encoding.UTF8));
            }
        });
    }
}
