using ActivityLog.Entities;

namespace ActivityLog.Services;

/// <summary>
/// Interface for accessing the activity log queue in a thread-safe manner
/// </summary>
public interface IActivityLogQueueAccessor
{
    /// <summary>
    /// Adds an activity log record to the queue
    /// </summary>
    /// <param name="record">The activity log record to enqueue</param>
    void Enqueue(ActivityLogRecord record);

    /// <summary>
    /// Attempts to remove and return an activity log record from the queue
    /// </summary>
    /// <param name="record">The dequeued record, if successful</param>
    /// <returns>True if a record was successfully dequeued; otherwise, false</returns>
    bool TryDequeue(out ActivityLogRecord record);

    /// <summary>
    /// Gets the current number of items in the queue
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Indicates whether the queue is empty
    /// </summary>
    bool IsEmpty { get; }
}
