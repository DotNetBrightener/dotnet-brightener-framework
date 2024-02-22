using System;
using DotNetBrightener.gRPC.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Classification;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

public partial class AutoGenerateProtoFileReceiver : ISyntaxContextReceiver
{
    public CodeGeneratorSchema CodeGeneratorSchema;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDec)
        {
            return;
        }

        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDec);

        if (classSymbol is ITypeSymbol typeSymbol)
        {
            var interfaces = typeSymbol.AllInterfaces;

            if (interfaces.All(itf => itf.Name != nameof(IGrpcServicesProvider)))
            {
                return;
            }
        }

        var    containingAssembly = classSymbol!.ContainingAssembly;
        string assemblyDirectory  = "";

        if (containingAssembly != null)
        {
            var fileLocations = containingAssembly.Locations.Select(_ => _.SourceTree?.FilePath);

            var className = classSymbol.Name;

            var assemblyPath = fileLocations.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == className);

            if (!string.IsNullOrEmpty(assemblyPath))
            {
                assemblyDirectory = assemblyPath.GetAssemblyPath();
            }
        }

        //if (!Debugger.IsAttached)
        //    Debugger.Launch();

        var compilation = semanticModel.Compilation;

        // Get the assembly name of the generated code
        var generatedAssemblyName = compilation.AssemblyName;

        var serviceTypesProp = classDec.Members
                                       .Cast<PropertyDeclarationSyntax>()
                                       .FirstOrDefault(_ => _.Identifier.ToString() ==
                                                            nameof(IGrpcServicesProvider.ServiceTypes));

        if (serviceTypesProp is null)
            return;

        var serviceTypesValue = serviceTypesProp.DescendantNodes()
                                                .OfType<CollectionExpressionSyntax>();

        foreach (var expressionSyntax in serviceTypesValue)
        {
            var typeOfDeclarations = expressionSyntax.Elements
                                                     .OfType<ExpressionElementSyntax>()
                                                     .Select(e => e.Expression)
                                                     .OfType<TypeOfExpressionSyntax>();

            foreach (var typeOfExpressionSyntax in typeOfDeclarations)
            {
                var extractedType = semanticModel.GetTypeInfo(typeOfExpressionSyntax.Type).Type;

                if (extractedType is ITypeSymbol typeS &&
                    typeS.ContainingNamespace.ToDisplayString() != "<global namespace>")
                {
                    var protoFileDefinition = TryProcessGrpcServiceFromType(typeS, generatedAssemblyName);

                    if (protoFileDefinition is null)
                        continue;

                    InitializeModelIfNeeded(assemblyDirectory);

                    CodeGeneratorSchema.ProtoFileDefinitions.Add(protoFileDefinition);
                }
            }
        }
    }

    private void InitializeModelIfNeeded(string assemblyDirectory)
    {
        CodeGeneratorSchema ??= new CodeGeneratorSchema
        {
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
            protoFileDefinition.Messages.ForEach(m => m.ProtoServiceNamespace = protoFileDefinition.ProtoServiceNamespace);

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
            var attributes = method.GetAttributes();

            var grpcToRestApiAttribute = attributes.FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() ==
                                                                           typeof(GrpcToRestApiAttribute).FullName);

            if (grpcToRestApiAttribute is not null)
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

                if (_allowedRestMethods.All(m => !m.Equals(methodValue, StringComparison.OrdinalIgnoreCase)))
                {
                    methodValue = "GET";
                }

                if (!string.IsNullOrEmpty(methodValue) &&
                    !string.IsNullOrEmpty(routeTemplateValue))
                {
                    grpcMethod.GenerateRestTranscoding = true;
                    grpcMethod.RestMethod              = methodValue.ToLower();
                    grpcMethod.RestRouteTemplate       = routeTemplateValue;
                }
            }

            var returnType = method.ReturnType;

            ProtoMessageDefinition responseType = ProtoMessageGenerator.GenerateMessageDefinition(returnType,
                                                                                                  protoFileDefinition,
                                                                                                  methodName,
                                                                                                  "Response");

            ProtoMessageDefinition requestType = null;


            // Generate request type from parameters
            var requestTypeName = $"{methodName}Request";

            var parameter       = parameters[0];
            var methodParamType = parameter.Type;

            if (methodParamType.IsReferenceType)
            {
                requestType = ProtoMessageGenerator.GenerateMessageDefinition(methodParamType,
                                                                              protoFileDefinition,
                                                                              methodName,
                                                                              "Request");
            }
            else
            {
                // methodParamType is not a class
                var paramType  = methodParamType.ToDisplayString();
                var isOptional = paramType.EndsWith("?");

                if (isOptional)
                {
                    paramType = paramType.TrimEnd('?');
                }

                requestType = new ProtoMessageDefinition
                {
                    IsSingleType = true,
                    ProtobufType = requestTypeName,
                    Fields =
                    [
                        new ProtoMessageFieldDefinition
                        {
                            Name         = parameter.Name,
                            CsType       = paramType,
                            ProtobufType = paramType.ToProtobuf(),
                            IsOptional   = isOptional
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
            .. protoFileDefinition.Methods.Select(_ => _.ResponseType),
            .. protoFileDefinition.Messages,
        ];

        protoFileDefinition.Messages = messages.Where(messageDefinition => messageDefinition is not null)
                                               .Distinct(new ProtoMessageComparer())
                                               .ToList();

        return protoFileDefinition;
    }
}