using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotNetBrightener.gRPC.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

public class AutoGenerateProtoFileReceiver : ISyntaxContextReceiver
{
    internal List<CodeGenerationInfo> Models = new();

    public AutoGenerateProtoFileReceiver()
    {
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDec)
        {
            return;
        }

        if (classDec.Identifier.ToString() == "GRPCEntitiesProvider")
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
                                var attributes = typeS.GetAttributes();

                                if (attributes.Any(attr => attr.AttributeClass?.Name == "GrpcServiceAttribute"))
                                {
                                    // Code to execute when the GrpcServiceAttribute is found
                                    ProcessGrpcService(typeS);
                                }


                                //Models.Add(new CodeGenerationInfo
                                //{
                                //    GrpcAssembly          = generatedAssemblyName,
                                //    GrpcNamespace         = $"{generatedAssemblyName}",
                                //    GrpcPackageName       = $"{typeS.Name}",
                                //    ProtoFilePath         = $"{Path.Combine(assemblyDirectory, "Protos")}",
                                //    ServiceFilePath       = $"{Path.Combine(assemblyDirectory, "Services")}",
                                //    GrpcAssemblyPath      = $"{assemblyDirectory}",
                                //    TargetEntity          = typeS.Name,
                                //    TargetEntityNamespace = typeS.ContainingNamespace.ToDisplayString(),
                                //    DataServiceNamespace  = dataServiceNamespace
                                //});
                            }
                        }
                    }
                }
            }
        }
    }

    private void ProcessGrpcService(ITypeSymbol typeS)
    {
        var methods = typeS.GetMembers().OfType<IMethodSymbol>();

        var grpcMethods = new List<GrpcMethod>();

        foreach (var method in methods)
        {
            // Access method properties
            var methodName = method.Name;
            var returnType = method.ReturnType;

            if (returnType is INamedTypeSymbol namedType && 
                namedType.ConstructedFrom.ToString() == "System.Threading.Tasks.Task<TResult>")
            {
                var typeArgument = namedType.TypeArguments.FirstOrDefault();
                if (typeArgument != null)
                {
                    // Export the type argument
                }
            }

            var parameters = method.Parameters;

            var grpcMethod = new GrpcMethod
            {
                MethodName = methodName,
                ReturnType = returnType.ToString(),
                Parameters = string.Join(", ", parameters.Select(p => $"{p.Type} {p.Name}"))
            };

            grpcMethods.Add(grpcMethod);
        }
    }
}

public class GrpcMethod
{
    public string MethodName { get; set; }
    public string ReturnType { get; set; }
    public string Parameters { get; set; }
}