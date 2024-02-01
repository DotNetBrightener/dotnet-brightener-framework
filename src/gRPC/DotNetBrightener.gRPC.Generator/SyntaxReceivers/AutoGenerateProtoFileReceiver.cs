using DotNetBrightener.gRPC.Generator.Templates;
using DotNetBrightener.gRPC.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

public class AutoGenerateProtoFileReceiver : ISyntaxContextReceiver
{
    internal CodeGenerationInfo Models;

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

        if (classDec.Identifier.ToString() == "GrpcServiceProvider")
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

            var members = classDec.Members;

            foreach (var memberDeclarationSyntax in members.Cast<FieldDeclarationSyntax>())
            {
                var typeInfo = semanticModel.GetTypeInfo(memberDeclarationSyntax.Declaration.Type);

                var valueInitializer = memberDeclarationSyntax.Declaration
                                                              .Variables
                                                              .First()
                                                              .Initializer!;

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
                                var protoFileDefinition = TryProcessGrpcServiceFromType(typeS, generatedAssemblyName);

                                if (protoFileDefinition is null)
                                    continue;

                                InitializeModelIfNeeded(assemblyDirectory);

                                Models.ProtoFileDefinitions.Add(protoFileDefinition);
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitializeModelIfNeeded(string assemblyDirectory)
    {
        Models ??= new CodeGenerationInfo
        {
            ProtoFilePath    = $"{Path.Combine(assemblyDirectory, "Protos")}",
            ServiceFilePath  = $"{Path.Combine(assemblyDirectory, "Services")}",
            GrpcAssemblyPath = $"{assemblyDirectory}",
        };
    }

    private static ProtoFileDefinition TryProcessGrpcServiceFromType(ITypeSymbol typeS, string generatedAssemblyName)
    {
        var attributes = typeS.GetAttributes();

        var grpcServiceAttribute =
            attributes.FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(GrpcServiceAttribute));

        if (grpcServiceAttribute is not null)
        {
            var serviceName = typeS.Name.TrimStart('I');

            var nameArgument = grpcServiceAttribute.NamedArguments
                                                   .FirstOrDefault(arg => arg.Key == nameof(GrpcServiceAttribute.Name));

            if (nameArgument.Value.Value is string name)
            {
                serviceName = name;
            }

            var protoFileDefinition = ProcessGrpcService(typeS, serviceName);


            protoFileDefinition.TargetAssemblyName    = generatedAssemblyName;
            protoFileDefinition.ReferencedServiceName = typeS.Name;

            return protoFileDefinition;
        }

        return null;
    }

    private static ProtoFileDefinition ProcessGrpcService(ITypeSymbol typeS, string serviceName)
    {
        var methods = typeS.GetMembers()
                           .OfType<IMethodSymbol>();

        var grpcMethods = new List<ProtoMethodDefinition>();

        var protoFileDefinition = new ProtoFileDefinition
        {
            PackageName           = $"{serviceName}Package",
            ServiceName           = serviceName,
            ProtoServiceNamespace = typeS.ContainingNamespace.ToDisplayString(),
            Methods               = grpcMethods,
        };

        foreach (var method in methods)
        {
            var methodName = method.Name;
            var parameters = method.Parameters;

            if (parameters.Length > 1)
            {
                continue;
            }

            var grpcMethod = new ProtoMethodDefinition
            {
                Name = methodName,
            };


            // Check if the method has the GrpcToRestApiAttribute
            var grpcToRestApiAttribute = method.GetAttributes()
                                               .FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(GrpcToRestApiAttribute));

            if (grpcToRestApiAttribute != null)
            {
                // Read the method and RouteTemplate values from the attribute
                var methodValue = grpcToRestApiAttribute.NamedArguments
                                                        .FirstOrDefault(arg => arg.Key == nameof(GrpcToRestApiAttribute
                                                                                                    .Method))
                                                        .Value.Value as string ?? "GET";


                var routeTemplateValue = grpcToRestApiAttribute.NamedArguments
                                                               .FirstOrDefault(arg => arg.Key ==
                                                                                      nameof(GrpcToRestApiAttribute
                                                                                                .RouteTemplate))
                                                               .Value.Value as string;


                if (!string.IsNullOrEmpty(methodValue) &&
                    !string.IsNullOrEmpty(routeTemplateValue))
                {
                    grpcMethod.GenerateRestTranscoding = true;
                    grpcMethod.RestMethod              = methodValue.ToLower();
                    grpcMethod.RestRouteTemplate       = routeTemplateValue;
                }
            }

            var returnType = method.ReturnType;

            ProtoMessageDefinition responseType = ProcessResponseType(returnType, protoFileDefinition, methodName);

            ProtoMessageDefinition requestType = null;


            // Generate request type from parameters
            var requestTypeName = $"{methodName}Request";

            var parameter  = parameters[0];
            var methodParamType = parameter.Type;

            if (methodParamType == null) continue;


            if (methodParamType.IsReferenceType)
            {
                // methodParamType is a class
                // Add your code here for handling class types
            }
            else
            {
                // methodParamType is not a class
                var paramType = methodParamType.ToDisplayString();

                requestType = new ProtoMessageDefinition
                {
                    Name = requestTypeName,
                    Fields =
                    [
                        new ProtoMessageFieldDefinition
                        {
                            Name = parameter.Name,
                            Type = paramType.ToProtobuf(),
                        }
                    ]
                };
            }

            if (responseType != null)
            {
                grpcMethod.ResponseType = responseType;
            }

            if (requestType != null)
            {
                grpcMethod.RequestType = requestType;
            }

            grpcMethods.Add(grpcMethod);
        }

        List<ProtoMessageDefinition> messages =
        [
            .. protoFileDefinition.Methods.Select(_ => _.RequestType),
            .. protoFileDefinition.Methods.Select(_ => _.ResponseType)
        ];

        protoFileDefinition.Messages = messages.Where(messageDefinition => messageDefinition is not null)
                                               .Distinct(new ProtoMessageComparer())
                                               .ToList();

        return protoFileDefinition;
    }

    private static ProtoMessageDefinition ProcessResponseType(ITypeSymbol            returnType,
                                                              ProtoFileDefinition    protoFileDefinition,
                                                              string                 methodName)
    {
        ProtoMessageDefinition responseType = null;

        if (returnType is INamedTypeSymbol namedType &&
            namedType.ConstructedFrom.ToString() == "System.Threading.Tasks.Task<TResult>")
        {
            var typeArgument = namedType.TypeArguments.FirstOrDefault();
            var isRepeated   = false;

            if (typeArgument != null)
            {
                // Check if typeArgument is List<> or IEnumerable<>
                if (typeArgument is INamedTypeSymbol namedTypeSymbol && 
                    namedTypeSymbol.IsGenericType)
                {
                    // Get the inner type of the collection
                    var innerType = namedTypeSymbol.TypeArguments.FirstOrDefault();

                    if (innerType != null)
                    {
                        typeArgument = innerType;
                    }

                    isRepeated = namedTypeSymbol.ConstructedFrom.ToString() == "System.Collections.Generic.List<T>" ||
                                 namedTypeSymbol.ConstructedFrom.ToString() == "System.Collections.Generic.IEnumerable<T>";
                }

            }
                

            if (typeArgument != null)
            {
                // Export the type argument
                var typeofType = typeArgument.GetType();

                if (isRepeated)
                {
                    var repeatedType = new ProtoMessageDefinition
                    {
                        Name = $"{typeArgument.Name}Response",
                        Fields = typeArgument.GetMembers()
                                             .OfType<IPropertySymbol>()
                                             .Select(f => new ProtoMessageFieldDefinition
                                              {
                                                  Name = f.Name,
                                                  Type = f.Type.ToDisplayString().ToProtobuf()
                                             })
                                             .ToList()
                    };
                    protoFileDefinition.Messages.Add(repeatedType);

                    responseType = new ProtoMessageDefinition
                    {
                        Name = $"{methodName}Response",
                        Fields = [
                            new ProtoMessageFieldDefinition
                            {
                                Name       = "items",
                                Type       = repeatedType.Name,
                                IsRepeated = true,
                            }
                        ]
                    };
                }
                else
                {
                    responseType = new ProtoMessageDefinition
                    {
                        Name = $"{typeArgument.Name}Response",
                        Fields = typeArgument.GetMembers()
                                             .OfType<IPropertySymbol>()
                                             .Select(f => new ProtoMessageFieldDefinition
                                              {
                                                  Name = f.Name,
                                                  Type = f.Type.ToDisplayString().ToProtobuf()
                                             })
                                             .ToList()
                    };
                }
            }
        }

        return responseType;
    }
}