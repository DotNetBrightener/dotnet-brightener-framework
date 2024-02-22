using DotNetBrightener.WebApi.GenericCRUD.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.WebApi.GenericCRUD.Generator.SyntaxReceivers;

public class AutoGenerateApiControllerSyntaxReceiver : ISyntaxContextReceiver
{
    internal List<CodeGenerationInfo> Models = new();

    public AutoGenerateApiControllerSyntaxReceiver()
    {
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDec)
        {
            return;
        }

        if (classDec.Identifier.ToString() == "CRUDWebApiGeneratorRegistration")
        {
            var semanticModel = context.SemanticModel;
            var classSymbol   = semanticModel.GetDeclaredSymbol(classDec);

            var    containingAssembly = classSymbol!.ContainingAssembly;
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
                return;

            var compilation = semanticModel.Compilation;

            // Get the assembly name of the generated code
            var generatedAssemblyName = compilation.AssemblyName;

            var    members              = classDec.Members;
            string dataServiceNamespace = "";
            foreach (var memberDeclarationSyntax in members.Cast<FieldDeclarationSyntax>())
            {
                var typeInfo = semanticModel.GetTypeInfo(memberDeclarationSyntax.Declaration.Type);

                var valueInitializer = memberDeclarationSyntax.Declaration
                                                                     .Variables
                                                                     .First()
                                                                     .Initializer!;

                if (typeInfo.Type?.ToString() == "System.Type")
                {
                    var typeofSyntax  = valueInitializer.Value as TypeOfExpressionSyntax;
                    var extractedType = semanticModel.GetTypeInfo(typeofSyntax.Type).Type;

                    if (extractedType is ITypeSymbol typeS)
                    {
                        dataServiceNamespace = $"{typeS.ContainingAssembly.Name}.Data";
                    }
                }

                if (typeInfo.Type?.ToString() == "System.Collections.Generic.List<System.Type>")
                {
                    var list = valueInitializer.Value as
                                   CollectionExpressionSyntax;

                    foreach (var expressionSyntax in list.Elements.OfType<ExpressionElementSyntax>())
                    {
                        if (expressionSyntax.Expression is TypeOfExpressionSyntax typeOfExpressionSyntax)
                        {
                            var extractedType = semanticModel.GetTypeInfo(typeOfExpressionSyntax.Type).Type;

                            if (extractedType is ITypeSymbol typeS &&
                                typeS.ContainingNamespace.ToDisplayString() != "<global namespace>")
                            {
                                Models.Add(new CodeGenerationInfo
                                {
                                    ControllerAssembly     = generatedAssemblyName,
                                    ControllerNamespace    = $"{generatedAssemblyName}.Controllers",
                                    ControllerPath         = $"{Path.Combine(assemblyDirectory, "Controllers")}",
                                    ControllerAssemblyPath = $"{assemblyDirectory}",
                                    TargetEntity           = typeS.Name,
                                    TargetEntityNamespace  = typeS.ContainingNamespace.ToDisplayString(),
                                    DataServiceNamespace   = dataServiceNamespace
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}