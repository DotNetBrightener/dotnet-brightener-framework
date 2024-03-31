using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace DotNetBrightener.gRPC;

internal static class ScalarValueTypeConverter
{
    internal static readonly Dictionary<string, string> CsToProtoTypeMap = new()
    {
        {"bool", "bool"},
        {"byte", "bytes"},
        {"sbyte", "bytes"},
        {"short", "int32"},
        {"ushort", "uint32"},
        {"int", "int32"},
        {"uint", "uint32"},
        {"long", "int64"},
        {"ulong", "uint64"},
        {"float", "float"},
        {"double", "double"},
        {"DateTime", "google.protobuf.Timestamp"},
        {"System.DateTime", "google.protobuf.Timestamp"},
        {"DateTimeOffset", "google.protobuf.Timestamp"},
        {"System.DateTimeOffset", "google.protobuf.Timestamp"},
        {"TimeSpan", "google.protobuf.Duration"},
        {"Guid", "google.protobuf.StringValue"},
        {"decimal", "float"},
        {"Dictionary", "map"}
    };


    internal static readonly Dictionary<string, string> ProtoToCsTypeMap = new()
    {
        {"bool", "bool"},
        {"bytes", "byte"},
        {"int32", "int"},
        {"uint32", "short"},
        {"int64", "long"},
        {"uint64", "ulong"},
        {"map", "Dictionary"},
    };

    public static string ToProtobuf(this string type)
    {
        if (!CsToProtoTypeMap.TryGetValue(type, out var protobufType))
        {
            if (type.StartsWith("Dictionary<"))
            {
                return type.Replace("Dictionary<", "map<");
            }

            if (type.StartsWith("System.Collections.Generic.Dictionary<"))
            {
                return type.Replace("System.Collections.Generic.Dictionary<", "map<");
            }

            protobufType = type;
        }

        return protobufType;
    }

    public static string ToCsType(this string type)
    {
        if (!ProtoToCsTypeMap.TryGetValue(type, out var csType))
        {
            if (type.StartsWith("map<"))
            {
                return type.Replace("map<", "System.Collections.Generic.Dictionary<");
            }

            csType = type;
        }

        return csType;
    }
}