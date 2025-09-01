using ActivityLog.Models;

namespace ActivityLog.Services;

/// <summary>
/// Interface for accessing and modifying the current activity log context during method execution
/// </summary>
public interface IActivityLogContextAccessor
{
    /// <summary>
    /// Gets the current method execution context
    /// </summary>
    /// <returns>The current context if active, null otherwise</returns>
    MethodExecutionContext? GetCurrentContext();

    /// <summary>
    /// Adds metadata to the current activity log context
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <param name="value">The metadata value</param>
    void AddMetadata(string key, object? value);

    /// <summary>
    /// Adds multiple metadata entries to the current activity log context
    /// </summary>
    /// <param name="metadata">Dictionary containing key-value pairs to add</param>
    void AddMetadata(Dictionary<string, object?> metadata);

    /// <summary>
    /// Gets metadata from the current activity log context
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <returns>The metadata value if found, null otherwise</returns>
    object? GetMetadata(string key);

    /// <summary>
    /// Gets all metadata from the current activity log context
    /// </summary>
    /// <returns>A dictionary containing all metadata</returns>
    Dictionary<string, object?> GetAllMetadata();

    /// <summary>
    /// Sets the activity name for the current context
    /// </summary>
    /// <param name="activityName">The new activity name</param>
    void SetActivityName(string activityName);

    /// <summary>
    /// Sets the activity description for the current context
    /// </summary>
    /// <param name="activityDescription">The new activity description</param>
    void SetActivityDescription(string activityDescription);

    /// <summary>
    /// Sets the description format for the current context
    /// </summary>
    /// <param name="descriptionFormat">The new description format</param>
    void SetDescriptionFormat(string descriptionFormat);

    /// <summary>
    /// Sets the target entity for the current context
    /// </summary>
    /// <param name="targetEntity">The target entity string</param>
    void SetTargetEntity(string? targetEntity);

    /// <summary>
    /// Sets the target entity id for the current context
    /// </summary>
    /// <param name="targetEntityId">The target entity string</param>
    void SetTargetEntityId(object? targetEntityId);

    /// <summary>
    /// Checks if there is an active activity log context
    /// </summary>
    /// <returns>True if there is an active context, false otherwise</returns>
    bool HasActiveContext();
}

/// <summary>
/// Implementation of IActivityLogContextAccessor using AsyncLocal for thread-safe context storage
/// </summary>
public class ActivityLogContextAccessor(IActivityLogSerializer serializer) : IActivityLogContextAccessor
{
    private static readonly AsyncLocal<MethodExecutionContext?> _currentContext = new();

    /// <summary>
    /// Gets or sets the current method execution context
    /// </summary>
    internal static MethodExecutionContext? CurrentContext
    {
        get => _currentContext.Value;
        set => _currentContext.Value = value;
    }

    public MethodExecutionContext? GetCurrentContext()
    {
        return _currentContext.Value;
    }

    public void AddMetadata(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        var context = _currentContext.Value;
        if (context == null)
        {
            // No active context - this is not an error, just ignore silently
            // This allows methods to call AddMetadata even when not intercepted
            return;
        }

        context.Metadata[key] = value;
    }

    public void AddMetadata(Dictionary<string, object?> metadata)
    {
        if (metadata == null)
            return; // Silently ignore null dictionary

        if (metadata.Count == 0)
            return; // Silently ignore empty dictionary

        var context = _currentContext.Value;
        if (context == null)
        {
            // No active context - this is not an error, just ignore silently
            // This allows methods to call AddMetadata even when not intercepted
            return;
        }

        // Add each key-value pair to the context
        foreach (var kvp in metadata)
        {
            // Skip null or empty keys
            if (string.IsNullOrWhiteSpace(kvp.Key))
                continue;

            context.Metadata[kvp.Key] = kvp.Value;
        }
    }

    public object? GetMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var context = _currentContext.Value;
        return context?.Metadata.TryGetValue(key, out var value) == true ? value : null;
    }

    public Dictionary<string, object?> GetAllMetadata()
    {
        var context = _currentContext.Value;
        return context?.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object?>();
    }

    public void SetActivityName(string activityName)
    {
        var context = _currentContext.Value;
        if (context == null)
            return; // No active context - ignore silently

        if (!string.IsNullOrWhiteSpace(activityName))
            context.ActivityName = activityName;
    }

    public void SetActivityDescription(string activityDescription)
    {
        var context = _currentContext.Value;
        if (context == null)
            return; // No active context - ignore silently

        context.ActivityDescription = activityDescription;
    }

    public void SetDescriptionFormat(string descriptionFormat)
    {
        var context = _currentContext.Value;
        if (context == null)
            return; // No active context - ignore silently

        context.DescriptionFormat = descriptionFormat;
    }

    public void SetTargetEntity(string? targetEntity)
    {
        var context = _currentContext.Value;
        if (context == null)
            return; // No active context - ignore silently

        context.TargetEntity = targetEntity ?? string.Empty;
    }

    public void SetTargetEntityId(object? targetEntityId)
    {
        var context = _currentContext.Value;
        if (context == null)
            return; // No active context - ignore silently

        context.TargetEntityId = serializer.SerializeReturnValue(targetEntityId);
    }

    public bool HasActiveContext()
    {
        return _currentContext.Value != null;
    }
}

/// <summary>
/// Static helper class for easy access to activity log context functionality
/// </summary>
public static class ActivityLogContext
{
    private static IActivityLogContextAccessor? _accessor;

    /// <summary>
    /// Sets the context accessor instance (used by DI container)
    /// </summary>
    /// <param name="accessor">The context accessor instance</param>
    internal static void SetAccessor(IActivityLogContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// Gets the current method execution context
    /// </summary>
    /// <returns>The current context if active, null otherwise</returns>
    public static MethodExecutionContext? GetCurrentContext()
    {
        return _accessor?.GetCurrentContext();
    }

    /// <summary>
    /// Adds metadata to the current activity log context
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <param name="value">The metadata value</param>
    public static void AddMetadata(string key, object? value)
    {
        _accessor?.AddMetadata(key, value);
    }

    /// <summary>
    /// Adds multiple metadata entries to the current activity log context
    /// </summary>
    /// <param name="metadata">Dictionary containing key-value pairs to add</param>
    public static void AddMetadata(Dictionary<string, object?> metadata)
    {
        _accessor?.AddMetadata(metadata);
    }

    /// <summary>
    /// Sets the activity name for the current context
    /// </summary>
    /// <param name="activityName">The new activity name</param>
    public static void SetActivityName(string activityName)
    {
        _accessor?.SetActivityName(activityName);
    }

    /// <summary>
    /// Sets the activity description for the current context
    /// </summary>
    /// <param name="activityDescription">The new activity description</param>
    public static void SetActivityDescription(string activityDescription)
    {
        _accessor?.SetActivityDescription(activityDescription);
    }

    /// <summary>
    /// Sets the description format for the current context
    /// </summary>
    /// <param name="descriptionFormat">The new description format</param>
    public static void SetDescriptionFormat(string descriptionFormat)
    {
        _accessor?.SetDescriptionFormat(descriptionFormat);
    }

    /// <summary>
    /// Sets the target entity for the current context
    /// </summary>
    /// <param name="targetEntity">The target entity string</param>
    public static void SetTargetEntity(string? targetEntity)
    {
        _accessor?.SetTargetEntity(targetEntity);
    }

    /// <summary>
    /// Sets the target entity for the current context
    /// </summary>
    /// <param name="targetEntityId">The target entity object</param>
    public static void SetTargetEntityId(object? targetEntityId)
    {
        _accessor?.SetTargetEntityId(targetEntityId);
    }

    /// <summary>
    /// Gets metadata from the current activity log context
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <returns>The metadata value if found, null otherwise</returns>
    public static object? GetMetadata(string key)
    {
        return _accessor?.GetMetadata(key);
    }

    /// <summary>
    /// Gets all metadata from the current activity log context
    /// </summary>
    /// <returns>A dictionary containing all metadata</returns>
    public static Dictionary<string, object?> GetAllMetadata()
    {
        return _accessor?.GetAllMetadata() ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Checks if there is an active activity log context
    /// </summary>
    /// <returns>True if there is an active context, false otherwise</returns>
    public static bool HasActiveContext()
    {
        return _accessor?.HasActiveContext() ?? false;
    }
}
