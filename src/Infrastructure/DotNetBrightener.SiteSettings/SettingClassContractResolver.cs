﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace DotNetBrightener.SiteSettings;

public class TypeOnlyContractResolver<T> : TypeOnlyContractResolver
{
    public TypeOnlyContractResolver(): base(typeof(T))
    {
        NamingStrategy = new CamelCaseNamingStrategy();
    }
}

public class TypeOnlyContractResolver : DefaultContractResolver
{
    private readonly Type _type;

    public TypeOnlyContractResolver(Type type)
    {
        _type     = type;
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        property.ShouldSerialize = instance => property.DeclaringType == _type;
        return property;
    }
}