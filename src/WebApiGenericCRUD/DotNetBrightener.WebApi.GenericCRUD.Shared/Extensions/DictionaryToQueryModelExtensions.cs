/***
 *
 * Copyright (c) 2021 DotNetBrightener.
 * Licensed under MIT.
 * Feel free to use!!
 * https://gist.github.com/dotnetbrightener/3554524b246e038395c19ca71094a6b4
 ***/

using System.Collections.Concurrent;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

internal static class ReflectionCaching
{
    private static readonly ConcurrentDictionary<string, MemberInfo> CachedMemberInfoItems = new();
    private static readonly ConcurrentDictionary<Type, MemberInfo[]> CachedTypeMembers     = new();

    public static MemberInfo RetrieveMemberInfo<TType>(string memberName)
    {
        return RetrieveMemberInfo(typeof(TType), memberName);
    }

    public static MemberInfo RetrieveMemberInfo(this Type objectType, string memberName)
    {
        var cacheKey = GetMemberInfoCacheKey(objectType, memberName);

        if (!CachedMemberInfoItems.TryGetValue(cacheKey, out var memberInfo))
        {
            var allMemberInfo = GetCacheMembers(objectType);

            memberInfo = allMemberInfo.FirstOrDefault(_ => _.Name == memberName);

            if (memberInfo == null)
            {
                memberInfo =
                    allMemberInfo.FirstOrDefault(_ => String.Equals(_.Name,
                                                                    memberName,
                                                                    StringComparison.InvariantCultureIgnoreCase));
            }

            if (memberInfo == null)
                return null;

            if (!CachedMemberInfoItems.ContainsKey(cacheKey))
                CachedMemberInfoItems.TryAdd(cacheKey, memberInfo);
        }

        return memberInfo;
    }

    private static MemberInfo [ ] GetCacheMembers(Type objectType)
    {
        if (!CachedTypeMembers.TryGetValue(objectType, out var allMembers))
        {
            allMembers = objectType.GetMembers();
            CachedTypeMembers.TryAdd(objectType, allMembers);
        }

        return allMembers;
    }

    private static string GetMemberInfoCacheKey(Type objectType, string memberName)
    {
        return objectType.FullName + "." + memberName;
    }
}