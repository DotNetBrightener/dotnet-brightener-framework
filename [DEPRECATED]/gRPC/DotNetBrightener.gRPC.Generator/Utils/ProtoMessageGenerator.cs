using DotNetBrightener.gRPC.Generator.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.gRPC.Generator.Utils;

internal static class ProtoMessageGenerator
{

    private static readonly Dictionary<string, ListTypeMapping> ListTypesMap =
        GeneratorContext.AvailableListTypes.ToDictionary(listTypeMapping => listTypeMapping.ConstructedFromString,
                                                         listTypeMapping => listTypeMapping);

    internal static ProtoMessageDefinition GenerateMessageDefinition(ITypeSymbol inputType,
                                                                    ProtoFileDefinition protoFileDefinition,
                                                                    string methodName = "",
                                                                    string messageTypeSuffix = "")
    {
        ProtoMessageDefinition outputType = null;
        ITypeSymbol typeArgument;
        var isRepeated = false;

        if (inputType is INamedTypeSymbol namedType &&
            (namedType.ConstructedFrom.ToString() == "System.Threading.Tasks.Task<TResult>" ||
             namedType.ConstructedFrom.ToString() == "Task<>"))
        {
            typeArgument = namedType.TypeArguments.FirstOrDefault();
        }
        else
        {
            typeArgument = inputType;
        }


        if (typeArgument != null)
        {
            // Check if typeArgument is List<> or IEnumerable<>
            if (typeArgument is INamedTypeSymbol namedTypeSymbol &&
                namedTypeSymbol.IsGenericType &&
                ListTypesMap.ContainsKey(namedTypeSymbol.ConstructedFrom.ToString()))
            {
                // Get the inner type of the collection
                var innerType = namedTypeSymbol.TypeArguments.FirstOrDefault();

                if (innerType != null)
                {
                    typeArgument = innerType;
                }

                isRepeated = true;
            }
        }

        if (typeArgument != null)
        {
            // Export the type argument
            var typeMembers = typeArgument.GetMembers()
                                          .ToList();

            // Get members from the base class if any
            var baseType = typeArgument.BaseType;

            while (baseType != null)
            {
                var baseTypeMembers = baseType.GetMembers();
                typeMembers.AddRange(baseTypeMembers);

                baseType = baseType.BaseType;
            }


            var propertySymbols = typeMembers
                                 .OfType<IPropertySymbol>()
                                 .Where(prop => !prop.IsReadOnly);

            var protobufType = methodName + messageTypeSuffix;

            if (string.IsNullOrEmpty(protobufType))
            {
                protobufType = typeArgument.Name + "Message";
            }

            if (isRepeated)
            {
                var properties = ExtractMessageFields(protoFileDefinition, propertySymbols);

                if (properties.Count == 0)
                {
                    return null;
                }

                var repeatedType = new ProtoMessageDefinition
                {
                    ProtobufType = typeArgument.Name + messageTypeSuffix,
                    CsType = typeArgument.Name,
                    Fields = properties
                };
                protoFileDefinition.Messages.Add(repeatedType);

                var targetCsTypeNamespace = typeArgument.ContainingNamespace.ToDisplayString();

                outputType = new ProtoMessageDefinition
                {
                    ProtobufType = protobufType,
                    CsType = $"List<{typeArgument.Name}>",
                    CsNamespace = targetCsTypeNamespace,
                    Fields =
                    [
                        new ProtoMessageFieldDefinition
                        {
                            Name = "items",
                            ProtobufType = repeatedType.ProtobufType,
                            CsType = typeArgument.Name,
                            IsRepeated = true,
                        }
                    ]
                };
            }
            else
            {
                var targetCsTypeNamespace = typeArgument.ContainingNamespace.ToDisplayString();
                var properties = ExtractMessageFields(protoFileDefinition, propertySymbols);

                if (properties.Count == 0)
                {
                    return null;
                }

                outputType = new ProtoMessageDefinition
                {
                    ProtobufType = protobufType,
                    CsType = typeArgument.Name,
                    CsNamespace = targetCsTypeNamespace,
                    Fields = properties
                };
            }
        }

        return outputType;
    }

    internal static List<ProtoMessageFieldDefinition> ExtractMessageFields(ProtoFileDefinition protoFileDefinition, IEnumerable<IPropertySymbol> propertySymbols)
    {
        var properties = new List<ProtoMessageFieldDefinition>();

        foreach (var f in propertySymbols)
        {
            var fieldType = f.Type.ToDisplayString();
            var isOptional = fieldType.EndsWith("?");

            if (isOptional)
            {
                fieldType = fieldType.TrimEnd('?');
            }

            if (f.Type.IsReferenceType &&
                fieldType != "string")
            {
                var symbolType = GenerateMessageDefinition(f.Type,
                                                           protoFileDefinition);

                if (symbolType is not null)
                    properties.Add(new ProtoMessageFieldDefinition
                    {
                        Name = f.Name,
                        CsType = fieldType,
                        ProtobufType = symbolType.ProtobufType,
                        IsOptional = isOptional
                    });
            }
            else
            {
                properties.Add(new ProtoMessageFieldDefinition
                {
                    Name = f.Name,
                    CsType = fieldType,
                    ProtobufType = fieldType.ToProtobuf(),
                    IsOptional = isOptional
                });
            }
        }

        return properties;
    }
}
