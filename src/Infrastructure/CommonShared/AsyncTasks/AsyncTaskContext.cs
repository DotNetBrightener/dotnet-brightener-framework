using Newtonsoft.Json;

namespace WebApp.CommonShared.AsyncTasks;

public class AsyncTaskContext
{
    public Guid TaskId { get; init; } = Guid.CreateVersion7();

    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public object Input { get; init; }

    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public dynamic Result { get; set; }

    public string TaskName { get; set; }

    public string TaskDescription { get; set; }

    public float? PercentComplete { get; set; }

    public int? ProcessedRecords { get; set; }

    public int? ErrorRecords { get; set; }

    public string CurrentStatus { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public long? CurrentUserId { get; set; }

    public string? UserUuid { get; set; }

    public string? Errors { get; set; }

    public TResult? GetResultAs<TResult>()
    {
        if (Result is TResult result)
            return result;

        return default;
    }
}