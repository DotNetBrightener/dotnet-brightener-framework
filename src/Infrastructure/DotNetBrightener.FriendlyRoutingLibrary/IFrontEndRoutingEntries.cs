using System;

namespace DotNetBrightener.FriendlyRoutingLibrary;

/// <summary>
///     The container of all routing entries for the friendly routing, provides the functionalities for interacting with the routing entries
/// </summary>
public interface IFrontEndRoutingEntries
{
    bool TryGetRoutingEntry(string path, out FrontEndRoutingEntry routingEntry);

    bool TryGetPath(string itemId, string targetType, out string path);

    bool TryGetPath<TTargetType>(string itemId, out string path);

    bool TryGetPath(string itemId, Type targetType, out string path);

    void AddEntry<TTargetType>(string itemId, string path);

    void AddEntry(string itemId, string path, Type targetType);

    void AddEntry(string itemId, string path, string targetTypeFullName);

    void RemoveEntry<TTargetType>(string itemId);

    void RemoveEntry(string itemId, Type targetType);

    void ClearAllEntries();
}