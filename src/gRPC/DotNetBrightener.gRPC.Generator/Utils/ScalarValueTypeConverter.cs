using System;
using System.Collections.Generic;

namespace DotNetBrightener.gRPC.Generator.Utils;

internal static class ScalarValueTypeConverter
{
    internal static readonly Dictionary<Type, string> ScalarTypeMap = new()
    {
        {typeof(bool), "bool"},
        {typeof(byte), "bytes"},
        {typeof(sbyte), "bytes"},
        {typeof(short), "int32"},
        {typeof(ushort), "uint32"},
        {typeof(int), "int32"},
        {typeof(uint), "uint32"},
        {typeof(long), "int64"},
        {typeof(ulong), "uint64"},
        {typeof(float), "float"},
        {typeof(double), "double"},
        {typeof(string), "string"},
        {typeof(byte[]), "bytes"},
        {typeof(DateTime), "google.protobuf.Timestamp"},
        {typeof(DateTimeOffset), "google.protobuf.Timestamp"},
        {typeof(TimeSpan), "google.protobuf.Duration"},
        {typeof(Guid), "google.protobuf.StringValue"},
        {typeof(decimal), "google.protobuf.StringValue"}
    };

    internal static readonly Dictionary<string, string> ScalarStringTypeMap = new()
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
        {"DateTimeOffset", "google.protobuf.Timestamp"},
        {"TimeSpan", "google.protobuf.Duration"},
        {"Guid", "google.protobuf.StringValue"},
        {"decimal", "float"}
    };

    public static string ToProtobuf(this Type type)
    {
        return ScalarTypeMap[type];
    }

    public static string ToProtobuf(this string type)
    {
        if (!ScalarStringTypeMap.TryGetValue(type, out var protobufType))
        {
            protobufType = type;
        }

        return protobufType;
    }
}