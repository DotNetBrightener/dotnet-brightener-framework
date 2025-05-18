using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebApi.GenericCRUD.Generator.Utils;

namespace WebApi.GenericCRUD.Generator.SyntaxReceivers;

public static class AutoGenerateApiControllerSyntaxReceiver
{
    // Static method for IIncrementalGenerator predicate
    public static bool IsCandidateForGeneration(SyntaxNode node, CancellationToken cancellationToken)
    {
        // Check if this is a class declaration
        if (node is not ClassDeclarationSyntax classDec)
        {
            return false;
        }

        // Check if this is the registration class we're looking for
        return classDec.Identifier.ToString() == "CRUDWebApiGeneratorRegistration";
    }

    // Static method for IIncrementalGenerator transform
    public static IEnumerable<CodeGenerationInfo> GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDec      = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol   = semanticModel.GetDeclaredSymbol(classDec);

        if (classSymbol == null)
        {
            return null;
        }

        var    containingAssembly = classSymbol.ContainingAssembly;
        string assemblyDirectory  = "";

        if (containingAssembly != null)
        {
            var assemblyPath = containingAssembly.Locations.FirstOrDefault()?.SourceTree?.FilePath;

            if (!string.IsNullOrEmpty(assemblyPath))
            {
                assemblyDirectory = assemblyPath.GetAssemblyPath();
            }
        }

        if (string.IsNullOrEmpty(assemblyDirectory))
            return null;

        var compilation           = semanticModel.Compilation;
        var generatedAssemblyName = compilation.AssemblyName;

        var                      members              = classDec.Members;
        string                   dataServiceNamespace = "";
        List<CodeGenerationInfo> modelsList           = new List<CodeGenerationInfo>();

        foreach (var memberDeclarationSyntax in members.OfType<FieldDeclarationSyntax>())
        {
            var typeInfo = semanticModel.GetTypeInfo(memberDeclarationSyntax.Declaration.Type);

            if (memberDeclarationSyntax.Declaration.Variables.FirstOrDefault()?.Initializer == null)
            {
                continue;
            }

            var valueInitializer = memberDeclarationSyntax.Declaration
                                                          .Variables
                                                          .First()
                                                          .Initializer;

            if (typeInfo.Type?.ToString() == "System.Type")
            {
                var typeofSyntax = valueInitializer.Value as TypeOfExpressionSyntax;

                if (typeofSyntax == null) continue;

                var extractedType = semanticModel.GetTypeInfo(typeofSyntax.Type).Type;

                if (extractedType is ITypeSymbol typeS)
                {
                    dataServiceNamespace = $"{typeS.ContainingAssembly.Name}.Data";
                }
            }

            if (typeInfo.Type?.ToString() == "System.Collections.Generic.List<System.Type>")
            {
                // Handle InitializerExpressionSyntax (older C# versions)
                if (valueInitializer.Value is InitializerExpressionSyntax initExpr)
                {
                    foreach (var expr in initExpr.Expressions)
                    {
                        if (expr is TypeOfExpressionSyntax typeOfExpr)
                        {
                            var extractedType = semanticModel.GetTypeInfo(typeOfExpr.Type).Type;

                            if (extractedType is ITypeSymbol typeS &&
                                typeS.ContainingNamespace.ToDisplayString() != "<global namespace>")
                            {
                                modelsList.Add(new CodeGenerationInfo
                                {
                                    TargetEntity           = typeS.Name,
                                    TargetEntityNamespace  = typeS.ContainingNamespace.ToDisplayString(),
                                    DataServiceNamespace   = dataServiceNamespace,
                                    ControllerAssemblyPath = assemblyDirectory,
                                    ControllerNamespace    = $"{generatedAssemblyName}.Controllers",
                                    ControllerPath         = Path.Combine(assemblyDirectory, "Controllers")
                                });
                            }
                        }
                    }
                }
                // Handle CollectionExpressionSyntax (C# 12+)
                else if (valueInitializer.Value is CollectionExpressionSyntax collExpr)
                {
                    foreach (var element in collExpr.Elements.OfType<ExpressionElementSyntax>())
                    {
                        if (element.Expression is TypeOfExpressionSyntax typeOfExpr)
                        {
                            var extractedType = semanticModel.GetTypeInfo(typeOfExpr.Type).Type;

                            if (extractedType is ITypeSymbol typeS &&
                                typeS.ContainingNamespace.ToDisplayString() != "<global namespace>")
                            {
                                modelsList.Add(new CodeGenerationInfo
                                {
                                    TargetEntity           = typeS.Name,
                                    TargetEntityNamespace  = typeS.ContainingNamespace.ToDisplayString(),
                                    DataServiceNamespace   = dataServiceNamespace,
                                    ControllerAssemblyPath = assemblyDirectory,
                                    ControllerNamespace    = $"{generatedAssemblyName}.Controllers",
                                    ControllerPath         = Path.Combine(assemblyDirectory, "Controllers")
                                });
                            }
                        }
                    }
                }
            }
        }

        return modelsList;
    }
}