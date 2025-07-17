using System.Collections.Concurrent;
using DotNetBrightener.Core.BackgroundTasks.Internal;

namespace DotNetBrightener.Core.BackgroundTasks;

public interface ILockedTasksContainer
{
    /// <summary>
    ///     Try to lock a key for a specified timeout
    /// </summary>
    /// <param name="key">The identifier of the lock</param>
    /// <param name="timeout">The time to lock the identifier for</param>
    /// <returns></returns>
    bool TryLock(string key, TimeSpan timeout);

    /// <summary>
    ///     Releases the lock applied to the given <see cref="key"/>
    /// </summary>
    /// <param name="key">The identifier of the lock</param>
    void Release(string key);
}

public class LockedTasksContainer(IDateTimeProvider dateTimeProvider) : ILockedTasksContainer
{
    private readonly        ConcurrentDictionary<string, LockedTaskInstance> _lockedTasks = new();
    private static readonly Lock                                             Lock         = new();

    public bool TryLock(string key, TimeSpan timeout)
    {
        lock (Lock)
        {
            if (_lockedTasks.TryGetValue(key, out var lockedInstance) &&
                lockedInstance.IsLocked &&
                lockedInstance.ExpiresAt < dateTimeProvider.UtcNow)
            {
                return false;
            }

            return CreateLockedInstance(key, timeout);
        }
    }

    public void Release(string key)
    {
        lock (Lock)
        {
            _lockedTasks.TryRemove(key, out _);
        }
    }

    private bool CreateLockedInstance(string key, TimeSpan timeout)
    {
        DateTime? expiresAt = dateTimeProvider.UtcNow.Add(timeout);

        if (_lockedTasks.TryGetValue(key, out var lockedInstance))
        {
            lockedInstance.IsLocked  = true;
            lockedInstance.ExpiresAt = expiresAt;
        }
        else
        {
            _lockedTasks.TryAdd(key,
                                new LockedTaskInstance
                                {
                                    IsLocked  = true,
                                    ExpiresAt = expiresAt
                                });
        }

        return true;
    }
}