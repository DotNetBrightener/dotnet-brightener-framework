using System.Text;
using DotNetBrightener.Mapper.Generators.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotNetBrightener.Mapper.Generators.WrapperGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class WrapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var wrappers = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.WrapperAttributeFullName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, token) => WrapperModelBuilder.BuildModel(ctx, token))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(wrappers, static (spc, model) =>
        {
            if (model is null) return;

            spc.CancellationToken.ThrowIfCancellationRequested();

            var code = WrapperCodeBuilder.Generate(model);
            spc.AddSource($"{model.FullName}.Wrapper.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }
}
