using System;
using System.Collections.Generic;

namespace DotNetBrightener.gRPC.Generator.Templates;

internal class ClassTypeTemplate
{
    internal const string ClassTypeContent = @"
using System;
using System.Collections.Generic;
using DotNetBrightener.gRPC.Extensions;
using Google.Protobuf.WellKnownTypes;
{MissingUsings}

// ReSharper disable CheckNamespace

namespace {ProtoServiceNamespace};

public static partial class {CsInputTypeClass}Converter
{
    public static {ProtoType} To{ProtoType}(this {CsInputType} other)
    {
        // TODO: The generator won't be able to generate the correct business logic for the conversion.
        // Please implement the conversion logic here.
        throw new NotImplementedException();    

        var result{ProtoType} = new {ProtoType}
        {{#each Fields}{/each}
        };

        return result{ProtoType};
    }

    public static {CsInputType} To{CsInputTypeClass}(this {ProtoType} other)
    {
        // TODO: The generator won't be able to generate the correct business logic for the conversion.
        // Please implement the conversion logic here.
        throw new NotImplementedException();

        var result{CsInputTypeClass} = new {CsInputType}
        {{#each CsFields}{/each}
        };

        return result{CsInputTypeClass};
    }
}";

    public static string Generate(ProtoMessageDefinition protoMessageDefinition)
    {
        if (protoMessageDefinition.IsSingleType)
        {
            return string.Empty;
        }

        string generatedCode = ClassTypeContent;

        generatedCode = generatedCode.Replace("{ProtoServiceNamespace}", protoMessageDefinition.ProtoServiceNamespace);
        generatedCode = generatedCode.Replace("{ProtoType}", protoMessageDefinition.ProtobufType);
        generatedCode = generatedCode.Replace("{CsInputType}", protoMessageDefinition.CsType);
        generatedCode = generatedCode.Replace("{CsInputTypeClass}",
                                              protoMessageDefinition.CsType
                                                                    .Replace("<", "")
                                                                    .Replace(">", "")
                                             );

        var fieldsCode = new List<string>();

        foreach (var field in protoMessageDefinition.Fields)
        {
            var rightOperand = $"other.{field.Name.ToTitleCase()}";

            if (field.CsType != field.ProtobufType)
            {
                var originalTypeCasted = field.ProtobufType.ToCsType();

                if (field.CsType != originalTypeCasted)
                {
                    if (field.ProtobufType == "google.protobuf.Timestamp")
                    {
                        var m = "FromDateTime";

                        if (field.CsType == "DateTimeOffset" ||
                            field.CsType == "System.DateTimeOffset")
                        {
                            m = "FromDateTimeOffset";
                        }

                        rightOperand = field.IsOptional
                                           ? $"other.{field.Name.ToTitleCase()}.HasValue ? Timestamp.{m}(other.{field.Name.ToTitleCase()}.Value) : null"
                                           : $"Timestamp.{m}(other.{field.Name.ToTitleCase()})";
                    }
                    else
                    {
                        rightOperand = $"({originalTypeCasted})other.{field.Name.ToTitleCase()}";
                    }
                }
            }

            if (field.IsOptional &&
                field.CsType == "string")
            {
                rightOperand += " ?? string.Empty";
            }

            fieldsCode.Add($@"
            {field.Name.ToTitleCase()} = {rightOperand}");
        }

        generatedCode = generatedCode.Replace(@"{#each Fields}{/each}",
                                              string.Join(",", fieldsCode));

        fieldsCode.Clear();

        foreach (var field in protoMessageDefinition.Fields)
        {
            var rightOperand = $"other.{field.Name.ToTitleCase()}";

            if (field.CsType != field.ProtobufType)
            {
                var originalTypeCasted = field.ProtobufType.ToCsType();

                if (field.CsType != originalTypeCasted)
                {
                    if (field.ProtobufType == "google.protobuf.Timestamp")
                    {
                        var m = "ToDateTime";

                        if (field.CsType == "DateTimeOffset" ||
                            field.CsType == "System.DateTimeOffset")
                        {
                            m = "ToDateTimeOffset";
                        }

                        rightOperand = field.IsOptional
                                           ? $"other.{field.Name.ToTitleCase()}?.{m}()"
                                           : $"other.{field.Name.ToTitleCase()}.{m}()";
                    }
                    else
                    {
                        rightOperand = $"({field.CsType})other.{field.Name.ToTitleCase()}";
                    }
                }
            }

            if (field.IsOptional &&
                field.CsType == "string")
            {
                rightOperand += " ?? string.Empty";
            }

            fieldsCode.Add($@"
            {field.Name.ToTitleCase()} = {rightOperand}");
        }

        generatedCode = generatedCode.Replace(@"{#each CsFields}{/each}",
                                              string.Join(",", fieldsCode));

        var missingUsings = new List<string>();

        if (!generatedCode.Contains($"using {protoMessageDefinition.CsNamespace};"))
        {
            missingUsings.Add($"using {protoMessageDefinition.CsNamespace};");
        }

        generatedCode = generatedCode.Replace("{MissingUsings}", string.Join(Environment.NewLine, missingUsings));

        return generatedCode;
    }
}