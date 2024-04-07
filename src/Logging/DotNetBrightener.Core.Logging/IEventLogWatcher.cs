namespace DotNetBrightener.Core.Logging;

/// <summary>
///     Represents the watcher which holds the log records written by <see cref="ILogger"/>
/// </summary>
public interface IEventLogWatcher
{
    /// <summary>
    ///     Retrieves the queued items for processing and remove them from queue.
    /// </summary>
    /// <returns>
    ///     List of event log records that got queued up as of the time of calling the method
    /// </returns>
    List<EventLogBaseModel> GetQueuedEventLogRecords();
}