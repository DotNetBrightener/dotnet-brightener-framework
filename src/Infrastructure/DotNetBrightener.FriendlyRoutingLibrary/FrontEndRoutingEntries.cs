namespace DotNetBrightener.FriendlyRoutingLibrary;

public class FrontEndRoutingEntries : IFrontEndRoutingEntries
{
    private readonly        Dictionary<string, FrontEndRoutingEntry> _pathsWithEntry;
    private readonly        Dictionary<string, FrontEndRoutingEntry> _pathsMapRouteEntries;
    private readonly        Type[]                                   _typeMetadatas;
    private static readonly Lock                                     Lock = new();

    public FrontEndRoutingEntries()
    {
        _typeMetadatas = AppDomain.CurrentDomain.GetAssemblies()
                                  .SelectMany(_ => _.GetTypes())
                                  .ToArray();

        _pathsWithEntry       = new Dictionary<string, FrontEndRoutingEntry>();
        _pathsMapRouteEntries = new Dictionary<string, FrontEndRoutingEntry>(StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetRoutingEntry(string path, out FrontEndRoutingEntry routingEntry)
    {
        return _pathsMapRouteEntries.TryGetValue(path, out routingEntry);
    }

    public bool TryGetPath(string itemId, string targetType, out string path)
    {
        var target = _typeMetadatas.FirstOrDefault(_ => _.FullName == targetType);

        if (target == null)
        {
            path = null;

            return false;
        }

        return TryGetPath(itemId, target, out path);
    }

    public bool TryGetPath<TTargetType>(string itemId, out string path)
    {
        return TryGetPath(itemId, typeof(TTargetType), out path);
    }

    public bool TryGetPath(string itemId, Type targetType, out string path)
    {
        if (_pathsWithEntry.TryGetValue(GetEntryKey(itemId, targetType), out var entry))
        {
            path = entry.Path;

            return true;
        }

        path = null;

        return false;
    }

    public void AddEntry<TTargetType>(string itemId, string path)
    {
        AddEntry(itemId, path, typeof(TTargetType));
    }

    public void AddEntry(string itemId, string path, string targetTypeFullName)
    {
        var target = _typeMetadatas.FirstOrDefault(_ => _.FullName == targetTypeFullName);

        if (target == null)
            return;

        AddEntry(itemId, path, target);
    }

    public void AddEntry(string itemId, string path, Type targetType)
    {
        lock (Lock)
        {
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            var entry = new FrontEndRoutingEntry
            {
                ItemId     = itemId,
                Path       = path,
                TargetType = targetType
            };
            _pathsWithEntry[GetEntryKey(itemId, targetType)] = entry;
            _pathsMapRouteEntries[path]                      = entry;
        }
    }

    public void RemoveEntry<TTargetType>(string itemId)
    {
        RemoveEntry(itemId, typeof(TTargetType));
    }

    public void RemoveEntry(string itemId, Type targetType)
    {
        lock (Lock)
        {
            var key = GetEntryKey(itemId, targetType);

            if (_pathsWithEntry.Remove(key, out var existingRecord))
            {
                _pathsMapRouteEntries.Remove(existingRecord.Path);
            }

        }
    }

    public void ClearAllEntries()
    {
        lock (Lock)
        {
            _pathsWithEntry.Clear();
            _pathsMapRouteEntries.Clear();
        }
    }

    private static string GetEntryKey(string itemId, Type targetType)
    {
        return targetType.FullName + "_" + itemId;
    }
}